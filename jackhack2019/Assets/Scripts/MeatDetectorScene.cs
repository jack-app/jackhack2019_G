using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;
using OpenCvSharp.Tracking;
using System.Linq;

public class MeatDetectorScene : WebCamera
{
    public TextAsset meats;

    public MeatProcessorLive<WebCamTexture> processor;

    public GameObject UIText;

    const float downScale = 0.5f;
    const float minimumAreaDiagonal = 20.0f;


    public List<DetectedMeat> meatlist = new List<DetectedMeat>();

    // tracker
    public List<Size> frameSize = new List<Size>();
    public List<Tracker> tracker = new List<Tracker>();

    protected override void Awake()
    {
        base.Awake();

        processor = new MeatProcessorLive<WebCamTexture>();
        processor.Initialize(meats.text);

        // data stabilizer - affects face rects, meat landmarks etc.
        processor.DataStabilizer.Enabled = true;        // enable stabilizer
        processor.DataStabilizer.Threshold = 2.0;       // threshold value in pixels
        processor.DataStabilizer.SamplesCount = 6;      // how many samples do we need to compute stable data

        // performance data - some tricks to make it work faster
        processor.Performance.Downscale = 512;          // processed image is pre-scaled down to N px by long side
        processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)

    }

    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {

        Mat image = OpenCvSharp.Unity.TextureToMat(input, TextureParameters);
        Mat downscaled = image.Resize(Size.Zero, downScale, downScale);
        Rect2d obj = Rect2d.Empty;

        int tempCount = processor.ProcessTexture(input, TextureParameters);
        if (meatlist.Count < tempCount)
        {
            Debug.Log(tempCount + " meat change");
            
            for(int i = 0; i < meatlist.Count; i++)
            {
                DropTracking(i);
            }

            meatlist = new List<DetectedMeat>(processor.Meats);

        }


        var areaRect = new OpenCvSharp.Rect();

        Debug.Log("meat count : " + meatlist.Count);

        // If not dragged - show the tracking data
        if (meatlist.Count > 0)
        {

            for(int i = 0; i < meatlist.Count; i++)
            {
                //Debug.LogFormat("meatCount : {0}, meatlist.Count : {1}, i : {2}", meatCount, meatlist.Count, i);
                DetectedMeat meat = meatlist[meatlist.Count - i - 1];

                // we have to tracker - let's initialize one
                if (tracker.Count <= i || tracker[i] == null)
                {
                    // but only if we have big enough "area of interest", this one is added to avoid "tracking" some 1x2 pixels areas
                    if (new Vector2(meat.Region.X, meat.Region.Y).magnitude >= minimumAreaDiagonal)
                    {
                        obj = new Rect2d(meat.Region.X, meat.Region.Y, meat.Region.Width, meat.Region.Height);

                        // initial tracker with current image and the given rect, one can play with tracker types here
                        if (tracker.Count <= i)
                        {
                            tracker.Add(Tracker.Create(TrackerTypes.MedianFlow));
                            Debug.Log("add tracker");
                        }
                        else if (tracker[i] == null)
                        {
                            Debug.Log("replace tracker");
                            tracker[i] = Tracker.Create(TrackerTypes.MedianFlow);
                        }
                        tracker[i].Init(downscaled, obj);

                        frameSize.Add(downscaled.Size());

                        var newVec = new Vector2((float)obj.X, -(float)obj.Y) - new Vector2(image.Width / 2.0f, -image.Height / 2.0f);

                        GameObject a = GameObject.Instantiate(UIText, gameObject.transform.parent);
                        a.transform.localPosition = (newVec + new Vector2((float)obj.Width / 2.0f, 0)) * gameObject.transform.localScale.x;
                    }
                }

                // if we already have an active tracker - just to to update with the new frame and check whether it still tracks object
                else
                {
                    // drop tracker if the frame's size has changed, this one is necessary as tracker doesn't hold it well
                    if (frameSize.Count > i && frameSize[i].Height != 0 && frameSize[i].Width != 0 && downscaled.Size() != frameSize[i])
                        DropTracking(i);

                    if (!tracker[i].Update(downscaled, ref obj))
                    {
                        Debug.Log("404 not found");

                        obj = Rect2d.Empty;

                        DropTracking(i);
                    }
                }

                // save tracked object location
                if (0 != obj.Width && 0 != obj.Height)
                    areaRect = new OpenCvSharp.Rect((int)obj.X, (int)obj.Y, (int)obj.Width, (int)obj.Height);

                // render rect we've tracker or one is being drawn by the user
                if (null != tracker[i] && obj.Width != 0)
                {
                    Cv2.Rectangle((InputOutputArray)(image), areaRect * (1.0 / downScale), Scalar.LightGreen, 4);
                    
                    //Cv2.PutText((InputOutputArray)(image), i.ToString("000"), new Point(areaRect.X, areaRect.Y), HersheyFonts.HersheySimplex, 2, Scalar.Yellow, 3);
                }
            }
        }

        // mark detected objects
        processor.MarkDetected(image);

        // processor.Image now holds data we'd like to visualize
        output = OpenCvSharp.Unity.MatToTexture(image, output);   // if output is valid texture it's buffer will be re-used, otherwise it will be re-created

        return true;
    }

    protected void DropTracking(int i)
    {
        Debug.Log(i + " removed");
        if (tracker[i] != null)
        {
            tracker[i].Dispose();
            tracker[i] = null;
        }
        meatlist.RemoveAt(i);
    }

}
