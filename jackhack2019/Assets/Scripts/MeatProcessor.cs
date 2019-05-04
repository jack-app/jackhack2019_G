using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;
using OpenCvSharp.Demo;
using System;

public class MeatProcessorPerformanceParams
{
    public int Downscale { get; set; }

    public int SkipRate { get; set; }

    public MeatProcessorPerformanceParams()
    {
        Downscale = 0;
        SkipRate = 0;
    }
}

public class MeatProcessor<T>
    where T: UnityEngine.Texture
{
    protected CascadeClassifier cascadeMeats = null;

    protected Mat processingImage = null;
    protected double appliedFactor = 1.0;

    public MeatProcessorPerformanceParams Performance { get; private set; }

    public DataStabilizerParams DataStabilizer { get; private set; }

    public Mat Image { get; private set; }

    public List<DetectedMeat> Meats { get; private set; }

    public MeatProcessor()
    {
        Meats = new List<DetectedMeat>();
        DataStabilizer = new DataStabilizerParams();
        Performance = new MeatProcessorPerformanceParams();
    }

    public virtual void Initialize(string meatsCascadeData)
    {
        // meat detector - the key thing here
        if (null == meatsCascadeData || meatsCascadeData.Length == 0)
            throw new Exception("MeatProcessor.Initialize: No meat detector cascade passed, with parameter is required");

        FileStorage storageMeats = new FileStorage(meatsCascadeData, FileStorage.Mode.Read | FileStorage.Mode.Memory);
        cascadeMeats = new CascadeClassifier();
        if (!cascadeMeats.Read(storageMeats.GetFirstTopLevelNode()))
            throw new System.Exception("MeatProcessor.Initialize: Failed to load meats cascade classifier");
    }

    protected virtual Mat MatFromTexture(T texture, OpenCvSharp.Unity.TextureConversionParams texParams)
    {
        if (texture is UnityEngine.Texture2D)
            return OpenCvSharp.Unity.TextureToMat(texture as UnityEngine.Texture2D, texParams);
        else if (texture is UnityEngine.WebCamTexture)
            return OpenCvSharp.Unity.TextureToMat(texture as UnityEngine.WebCamTexture, texParams);
        else
            throw new Exception("MeatProcessor: incorrect input texture type, must be Texture2D or WebCamTexture");
    }

    protected virtual void ImportTexture(T texture, OpenCvSharp.Unity.TextureConversionParams texParams)
    {
        // free currently used textures
        if (null != processingImage)
            processingImage.Dispose();
        if (null != Image)
            Image.Dispose();

        // convert and prepare
        Image = MatFromTexture(texture, texParams);
        if (Performance.Downscale > 0 && (Performance.Downscale < Image.Width || Performance.Downscale < Image.Height))
        {
            // compute aspect-respective scaling factor
            int w = Image.Width;
            int h = Image.Height;

            // scale by max side
            if (w >= h)
            {
                appliedFactor = (double)Performance.Downscale / (double)w;
                w = Performance.Downscale;
                h = (int)(h * appliedFactor + 0.5);
            }
            else
            {
                appliedFactor = (double)Performance.Downscale / (double)h;
                h = Performance.Downscale;
                w = (int)(w * appliedFactor + 0.5);
            }

            // resize
            processingImage = new Mat();
            Cv2.Resize(Image, processingImage, new Size(w, h));
        }
        else
        {
            appliedFactor = 1.0;
            processingImage = Image;
        }
    }

    public virtual int ProcessTexture(T texture, OpenCvSharp.Unity.TextureConversionParams texParams, bool detect = true)
    {
        // convert Unity texture to OpenCv::Mat
        ImportTexture(texture, texParams);

        // detect
        if (detect)
        {
            double invF = 1.0 / appliedFactor;
            DataStabilizer.ThresholdFactor = invF;

            // convert to grayscale and normalize
            Mat gray = new Mat();
            Cv2.CvtColor(processingImage, gray, ColorConversionCodes.BGR2GRAY);

            // fix shadows
            Cv2.EqualizeHist(gray, gray);

            /*Mat normalized = new Mat();
            CLAHE clahe = CLAHE.Create();
            clahe.TilesGridSize = new Size(8, 8);
            clahe.Apply(gray, normalized);
            gray = normalized;*/

            // detect matching regions (meats bounding)
            Rect[] rawMeats = cascadeMeats.DetectMultiScale(gray, 1.2, 6);
            if (Meats.Count != rawMeats.Length)
                Meats.Clear();

            for (int i = 0; i < rawMeats.Length; ++i)
            {
                Rect meatRect = rawMeats[i];
                Rect meatRectScaled = meatRect * invF;
                using (Mat grayMeat = new Mat(gray, meatRect))
                {

                    // get meat object
                    DetectedMeat meat = null;
                    if (Meats.Count < i + 1)
                    {
                        meat = new DetectedMeat(DataStabilizer, meatRectScaled);
                        Meats.Add(meat);
                    }
                    else
                    {
                        meat = Meats[i];
                        meat.SetRegion(meatRectScaled);
                    }

                }
            }
            // log
            //UnityEngine.Debug.Log(String.Format("Found {0} meats", Meats.Count));
        }

        return Meats.Count;
    }

    public void MarkDetected(Mat image)
    {
        // mark each found meat
        foreach (DetectedMeat meat in Meats)
        {
            // face rect
            Cv2.Rectangle((InputOutputArray)image, meat.Region, Scalar.FromRgb(255, 0, 0), 2);

        }
    }
}

public class MeatProcessorLive<T> : MeatProcessor<T>
        where T : UnityEngine.Texture
{
    private int frameCounter = 0;

    /// <summary>
    /// Constructs meat processor
    /// </summary>
    public MeatProcessorLive()
        : base()
    { }

    /// <summary>
    /// Detector
    /// </summary>
    /// <param name="inputTexture">Input Unity texture</param>
    /// <param name="texParams">Texture parameters (flipped, rotated etc.)</param>
    /// <param name="detect">Flag signalling whether we need detection on this frame</param>
    public override int ProcessTexture(T texture, OpenCvSharp.Unity.TextureConversionParams texParams, bool detect = true)
    {
        bool acceptedFrame = (0 == Performance.SkipRate || 0 == frameCounter++ % Performance.SkipRate);
        return base.ProcessTexture(texture, texParams, detect && acceptedFrame);
    }
}