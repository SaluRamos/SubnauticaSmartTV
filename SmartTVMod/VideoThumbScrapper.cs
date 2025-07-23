using FFmpeg.AutoGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Valve.VR;

namespace SmartTV
{

    //install the package with: Install-Package FFmpeg.AutoGen
    public class VideoThumbScrapper : MonoBehaviour
    {

        private static bool loadedFFMPEG = false;
        private static Dictionary<string, Video> map = new Dictionary<string, Video>();
        private static int TargetWidth = 158;
        private static int TargetHeight = 88;
        private static int TotalFramesInCache = 50;

        private class Video
        {
            public Texture2D[] texs = new Texture2D[TotalFramesInCache];
        }

        void Start()
        { 
            if (!loadedFFMPEG)
            {
                ffmpeg.RootPath = SmartTVMain.modFolder;
                loadedFFMPEG = true;
            }
            StartCoroutine(LoadAllVideosOneByOne());
        }

        private IEnumerator LoadAllVideosOneByOne()
        { 
            foreach (string url in SmartTVMain.videos)
            {
                GenerateThumbnailsFromVideo(url);
                yield return null;
            }
        }

        public static Texture2D GetFrame(string url, double time)
        {
            int index = 0;
            try
            {
                if (double.IsNaN(time))
                {
                    return null;
                }
                if (time < 0 || time > 1)
                {
                    Debug.LogError($"Time {time} is out of bounds. Must be between 0 and 1.");
                    return null;
                }
                if (!map.ContainsKey(url))
                {
                    Debug.LogError($"Video not loaded: {url}");
                    return null;
                }
                Video video = map[url];
                index = (int) Clamp(Math.Round(time * (TotalFramesInCache - 1)), 0, TotalFramesInCache - 1);
                if (video.texs.Length - 1 < index)
                {
                    Debug.LogError($"Index {index} out of bounds for video {url}. Total frames: {video.texs.Length}");
                    return null;
                }
                return video.texs[index];
            }
            catch (Exception e)
            { 
                Debug.LogError($"Error getting frame for {url}, index {index} at time {time}: {e.Message}");
            }
            return null;
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public unsafe void GenerateThumbnailsFromVideo(string url)
        {
            AVFormatContext* pFormatContext = null;
            AVCodecContext* pCodecContext = null;
            AVFrame* pFrame = null;
            AVFrame* pSoftwareFrame = null;
            AVPacket* pPacket = null;
            SwsContext* pSwsContext = null;
            try
            {
                map[url] = new Video();
                // 1. Abrir o arquivo de vídeo
                pFormatContext = ffmpeg.avformat_alloc_context();
                var pFormatContextRef = &pFormatContext;
                ThrowExceptionIfError(ffmpeg.avformat_open_input(pFormatContextRef, url, null, null), "Não foi possível abrir o arquivo");
                ThrowExceptionIfError(ffmpeg.avformat_find_stream_info(pFormatContext, null), "Não foi possível encontrar informações do stream");
                // 2. Encontrar o stream de vídeo e o codec
                AVCodec* pCodec = null;
                int videoStreamIndex = ffmpeg.av_find_best_stream(pFormatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, &pCodec, 0);
                ThrowExceptionIfError(videoStreamIndex, "Não foi possível encontrar um stream de vídeo");
                AVStream* pStream = pFormatContext->streams[videoStreamIndex];
                // 3. CONFIGURAR ACELERAÇÃO DE HARDWARE (GPU - Exemplo CUDA)
                AVHWDeviceType hwDeviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_CUDA; // Outras: AV_HWDEVICE_TYPE_QSV, AV_HWDEVICE_TYPE_DXVA2
                // Encontrar o decoder acelerado por hardware (ex: h264_cuvid)
                string decoderName = $"{ffmpeg.avcodec_get_name(pCodec->id)}_cuvid";
                var hwCodec = ffmpeg.avcodec_find_decoder_by_name(decoderName);
                if (hwCodec != null)
                {
                    pCodec = hwCodec; // Usar o codec de GPU se encontrado
                    Console.WriteLine($"Usando decoder de GPU: {decoderName}");
                }
                else
                {
                    Console.WriteLine($"Decoder de GPU não encontrado, usando CPU: {ffmpeg.avcodec_get_name(pCodec->id)}");
                }
                pCodecContext = ffmpeg.avcodec_alloc_context3(pCodec);
                ThrowExceptionIfError(ffmpeg.avcodec_parameters_to_context(pCodecContext, pStream->codecpar), "Falha ao copiar parâmetros do codec");
                // Se estivermos usando GPU, precisamos inicializar o device
                if (hwCodec != null)
                {
                    AVBufferRef* hwDeviceCtx = null;
                    ThrowExceptionIfError(ffmpeg.av_hwdevice_ctx_create(&hwDeviceCtx, hwDeviceType, null, null, 0), "Falha ao criar contexto de dispositivo de hardware (GPU)");
                    pCodecContext->hw_device_ctx = ffmpeg.av_buffer_ref(hwDeviceCtx);
                }
                // 4. Abrir o codec
                ThrowExceptionIfError(ffmpeg.avcodec_open2(pCodecContext, pCodec, null), "Não foi possível abrir o codec");
                // 5. BUSCAR (SEEK) PARA O FRAME DESEJADO
                long totalFrames = pStream->nb_frames;
                if (totalFrames <= 0) // Se o container não informa o total, calcula pela duração
                {
                    totalFrames = (long)(pStream->duration * ffmpeg.av_q2d(pStream->time_base) * pStream->avg_frame_rate.num / pStream->avg_frame_rate.den);
                }
                if (totalFrames <= 0) throw new ApplicationException("Não foi possível determinar o número de frames.");

                var framesToCapture = new HashSet<long>();
                var frameIndexMap = new Dictionary<long, int>();
                for (int i = 0; i < TotalFramesInCache; i++)
                {
                    long frameIndex = (long)((i / (double)TotalFramesInCache) * totalFrames);
                    if (framesToCapture.Add(frameIndex))
                    {
                        frameIndexMap[frameIndex] = i;
                    }
                }

                // 5. ALOCAR MEMÓRIA PARA OS FRAMES
                pFrame = ffmpeg.av_frame_alloc();
                pSoftwareFrame = ffmpeg.av_frame_alloc(); // Para cópia da GPU->CPU
                pPacket = ffmpeg.av_packet_alloc();
                long currentFrameIndex = -1;
                // 6. O LOOP PRINCIPAL DE LEITURA E DECODIFICAÇÃO
                while (ffmpeg.av_read_frame(pFormatContext, pPacket) >= 0)
                {
                    if (pPacket->stream_index != videoStreamIndex)
                    {
                        ffmpeg.av_packet_unref(pPacket);
                        continue;
                    }
                    if (ffmpeg.avcodec_send_packet(pCodecContext, pPacket) != 0) continue;
                    while (ffmpeg.avcodec_receive_frame(pCodecContext, pFrame) == 0)
                    {
                        currentFrameIndex++;
                        if (framesToCapture.Contains(currentFrameIndex))
                        {
                            int cacheIndex = frameIndexMap[currentFrameIndex];
                            AVFrame* frameToConvert = pFrame;
                            if (pFrame->hw_frames_ctx != null)
                            {
                                if (ffmpeg.av_hwframe_transfer_data(pSoftwareFrame, pFrame, 0) < 0) continue;
                                frameToConvert = pSoftwareFrame;
                            }
                            pSwsContext = ffmpeg.sws_getCachedContext(pSwsContext,
                                pCodecContext->width, pCodecContext->height, (AVPixelFormat)frameToConvert->format,
                                TargetWidth, TargetHeight, AVPixelFormat.AV_PIX_FMT_RGBA,
                                ffmpeg.SWS_BILINEAR, null, null, null);

                            var outputBuffer = new byte[TargetWidth * TargetHeight * 4];
                            fixed (byte* pOutputBuffer = outputBuffer)
                            {
                                var destData = new byte_ptrArray4 { [0] = pOutputBuffer };
                                var destLineSize = new int_array4 { [0] = TargetWidth * 4 };
                                ffmpeg.sws_scale(pSwsContext, frameToConvert->data, frameToConvert->linesize, 0, pCodecContext->height, destData, destLineSize);
                            }
                            map[url].texs[cacheIndex] = new Texture2D(TargetWidth, TargetHeight, TextureFormat.RGBA32, false);
                            map[url].texs[cacheIndex].LoadRawTextureData(outputBuffer);
                            map[url].texs[cacheIndex].Apply();
                            framesToCapture.Remove(currentFrameIndex);
                        }
                    }
                    ffmpeg.av_packet_unref(pPacket);
                    if (framesToCapture.Count == 0) break;
                }
            }
            finally
            {
                // 9. LIMPAR TODOS OS RECURSOS
                if (pSwsContext != null) ffmpeg.sws_freeContext(pSwsContext);
                if (pFrame != null) ffmpeg.av_frame_free(&pFrame);
                if (pSoftwareFrame != null) ffmpeg.av_frame_free(&pSoftwareFrame);
                if (pPacket != null) ffmpeg.av_packet_free(&pPacket);
                if (pCodecContext != null) ffmpeg.avcodec_free_context(&pCodecContext);
                if (pFormatContext != null)
                {
                    var pFormatContextRef = &pFormatContext;
                    ffmpeg.avformat_close_input(pFormatContextRef);
                }
            }
        }

        private unsafe static void ThrowExceptionIfError(int result, string message)
        {
            if (result < 0)
            {
                byte[] buffer = new byte[1024];
                fixed (byte* pBuffer = buffer)
                {
                    ffmpeg.av_strerror(result, pBuffer, (ulong)buffer.Length);
                    var error = System.Text.Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                    throw new ApplicationException($"{message}. Erro FFmpeg: {result} - {error}");
                }
            }
        }

    }
}
