using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AutoHelm.user_controls;
using AutoHelm.UserControls.DragAndDrop;
using Automation_Project.src.ast;
using Automation_Project.src.automation;
using Emgu.CV.Structure;
using Emgu.CV;
using Tesseract;
using System.Drawing.Drawing2D;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using static SymSpell;
using System.Drawing;
using System.Windows.Forms;

namespace Ocr
{
    public class OcrController
    {

        //Qualitative Member Variables
        private int controllerId = 0;
        private string file = "";
        private string keyword = "";
        private int imageScaler = 3;
        private int kernelSmoothingSize = 1;
        private int sigmaSmoothingSize = 0;
        private int threshBlockSize = 31;
        private double threshC = 0.03;
        private static int numControllers = 0;
        private String whiteList = "qwertyuioplkjhgfdsazxcvbnmQWERTYUIOPLKJHGFDSAZXCVBNM.,?!()\n\t ";
        private bool shouldStringMatch = true;

        //Quantitative Member Variables
        private Emgu.CV.CvEnum.Inter resizeMode = Emgu.CV.CvEnum.Inter.Cubic;
        private Emgu.CV.CvEnum.AdaptiveThresholdType threshType = Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC;
        private bool isDarkMode = false;

        //Different Constructors For Different Scenarios
        //This constructor assumes default parameters 
        public OcrController(string filePath, string keyword, bool darkMode)
        {
            this.file = filePath;
            this.keyword = keyword;
            this.isDarkMode = darkMode;
            this.controllerId = numControllers;
            this.shouldStringMatch = true;
            numControllers++;
        }

        //Constructor with custom image scaling
        public OcrController(string filePath, string keyword, int scale, Emgu.CV.CvEnum.Inter mode, bool darkMode, bool sm)
        {
            this.file = filePath;
            this.keyword = keyword;
            this.imageScaler = scale;
            this.resizeMode = mode;
            this.isDarkMode = darkMode;
            this.controllerId = numControllers;
            this.shouldStringMatch = sm;
            numControllers++;
        }

        //Constructor with custom smoothing
        public OcrController(string filePath, string keyword, int kernelSize, int sigmaSize, bool darkMode, bool sm)
        {
            this.file = filePath;
            this.keyword = keyword;
            this.kernelSmoothingSize = kernelSize;
            this.sigmaSmoothingSize = sigmaSize;
            this.isDarkMode = darkMode;
            this.controllerId = numControllers;
            this.shouldStringMatch = sm;
            numControllers++;
        }

        //Constructor for custom thresholding
        public OcrController(string filePath, string keyword, int blockSize, int c, Emgu.CV.CvEnum.AdaptiveThresholdType threshMode, bool darkMode, bool sm)
        {
            this.file = filePath;
            this.keyword = keyword;
            this.threshBlockSize = blockSize;
            this.threshC = c;
            this.threshType = threshMode;
            this.isDarkMode = darkMode;
            this.shouldStringMatch = sm;
            this.controllerId = numControllers;
            numControllers++;
        }

        public OcrController(string filePath, string keyword, int scale, int kernelSize, int sigmaSize, Emgu.CV.CvEnum.Inter sizeMode, int blockSize, int c, Emgu.CV.CvEnum.AdaptiveThresholdType threshMode, bool darkMode, bool sm)
        {
            this.file = filePath;
            this.keyword = keyword;
            this.imageScaler = scale;
            this.kernelSmoothingSize = kernelSize;
            this.sigmaSmoothingSize = sigmaSize;
            this.resizeMode = sizeMode;
            this.threshBlockSize = blockSize;
            this.threshC = c;
            this.threshType = threshMode;
            this.isDarkMode = darkMode;
            this.controllerId = numControllers;
            this.shouldStringMatch = sm;
            numControllers++;
        }

        //Functions for Intermediate OCR Preprocessing
        private Image<Bgr, byte> getImage()
        {
            if (!this.file.Equals(""))
            {
                return new Image<Bgr, byte>(this.file);
            }
            else
            {
                System.Drawing.Rectangle screenBounds = Screen.GetBounds(System.Drawing.Point.Empty);
                using (Bitmap bitmap = new Bitmap(screenBounds.Width, screenBounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(0, 0, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
                    }

                    bitmap.Save("screenshot.png", System.Drawing.Imaging.ImageFormat.Png);
                    return new Image<Bgr, byte>("screenshot.png");

                }

            }

        }

        private Image<Gray, Byte> getGrayScale(Image<Bgr, byte> image)
        {
            return image.Convert<Gray, Byte>();
        }

        private Image<Gray, Byte> resizeImage(Image<Gray, Byte> image)
        {
            return image.Resize(image.Width * imageScaler, image.Height * imageScaler, resizeMode);
        }

        private Image<Gray, Byte> blurImage(Image<Gray, Byte> image)
        {
            return image.SmoothGaussian(kernelSmoothingSize, kernelSmoothingSize, sigmaSmoothingSize, sigmaSmoothingSize);
        }

        private void performThresholding(Image<Gray, Byte> image)
        {
            if (!isDarkMode)
            {
                CvInvoke.AdaptiveThreshold(image, image, 255, threshType, Emgu.CV.CvEnum.ThresholdType.Binary, threshBlockSize, threshC);
            }
            else
            {
                CvInvoke.AdaptiveThreshold(image, image, 255, threshType, Emgu.CV.CvEnum.ThresholdType.BinaryInv, threshBlockSize, threshC);
            }

        }

        private void saveImage(Image<Gray, Byte> image)
        {
            Console.WriteLine(System.AppDomain.CurrentDomain.BaseDirectory + "\\" + "Grey" + "_" + controllerId.ToString() + ".PNG");
            image.Save(System.AppDomain.CurrentDomain.BaseDirectory + "\\" + "Grey" + "_" + controllerId.ToString() + ".PNG");
        }

        //Allows users to change the whitelist from the default one
        public void setOcrWhitelist(String wl)
        {
            this.whiteList = wl;
        }

        //Compare the similarilty betweena select word and an OCR word
        private int getStringSim(String one, String two)
        {
            String lower_one = one.ToLower();
            String lower_two = two.ToLower();

            if (lower_one.Equals(lower_two))
            {
                return one.Length;
            }
            else if (lower_two.IndexOf(lower_one) != -1)
            {
                return lower_one.Length - 1;
            }

            else
            {
                int score = 0;

                int[] hashMap1 = new int[26];
                int[] hashMap2 = new int[26];

                for (int i = 0; i < lower_one.Length; i++)
                {
                    hashMap1[lower_one[i] % 26] = 1;
                }

                for (int i = 0; i < lower_two.Length; i++)
                {
                    hashMap2[lower_two[i] % 26] = 1;
                }

                for (int i = 0; i < 26; i++)
                {
                    if ((hashMap1[i] == 1) && (hashMap2[i] == 1))
                    {
                        score++;
                    }
                }

                return score - 26;

            }
        }

        /*
        private string performStringMatching(string text)
        {
            if (!shouldStringMatch)
                return "";

            SymSpell symSpell = new SymSpell(text.Length, 2);
            int maxEditDistanceLookup = 2;
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dictionaryPath = baseDirectory + "frequency_dictionary_en_82_765.txt";
            int termIndex = 0; //column of the term in the dictionary text file
            int countIndex = 1; //column of the term frequency in the dictionary text file
            if (!symSpell.LoadDictionary(dictionaryPath, termIndex, countIndex))
            {
                return "";
            }
            var suggestions = symSpell.LookupCompound(text, maxEditDistanceLookup);
            return suggestions[0].term;
        }
        */

        /*
           string stringMatchingRet = performStringMatching(text);

           if (stringMatchingRet.Length > 0)
           {
               return stringMatchingRet;
           }
           else
           {
               return text;
           }
           */

        //Function to Perform OCR
        private (string, string, int, int) ocr()
        {
            string ret = "";
            string closest = "";
            int bestScore = Int32.MinValue;
            Tesseract.Rect boundingbox = new Tesseract.Rect();

            //Setup Tesseract Engine
            var img = Pix.LoadFromFile(System.AppDomain.CurrentDomain.BaseDirectory + "\\" + "Grey" + "_" + controllerId.ToString() + ".PNG");
            var ocr = new TesseractEngine(System.AppDomain.CurrentDomain.BaseDirectory + "\\ocrResources", "eng", EngineMode.Default);
            ocr.SetVariable("tessedit_char_whitelist", whiteList);

            //Setup String Matching Engine
            bool shouldPerformStringMatching = true;
            const int initialCapacity = 82765;
            const int maxEditDistance = 2;
            const int prefixLength = 7;
            SymSpell symSpell = new SymSpell(initialCapacity, maxEditDistance, prefixLength);
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dictionaryPath = baseDirectory + "frequency_dictionary_en_82_765.txt";
            int termIndex = 0; //column of the term in the dictionary text file
            int countIndex = 1; //column of the term frequency in the dictionary text file
            if (!this.shouldStringMatch || !symSpell.LoadDictionary(dictionaryPath, termIndex, countIndex))
            {
                shouldPerformStringMatching = false;
            }

            //Sring matching warmup as advised by documentation
            symSpell.Lookup("warmup", SymSpell.Verbosity.All);

            //Perform Word by word OCR
            Tesseract.PageIteratorLevel level = PageIteratorLevel.Word;
            using (var page = ocr.Process(img))
            using (var iter = page.GetIterator())
            {
                iter.Begin();
                do
                {
                    if (iter.TryGetBoundingBox(level, out var coordinateBox))
                    {
                        var text = iter.GetText(level).Trim();
                        ret += (text + " ");

                        if (shouldPerformStringMatching)
                        {
                            var suggestions = symSpell.Lookup(text, SymSpell.Verbosity.Closest);

                            if (suggestions.Count > 0 && suggestions[0].term.Length > 0)
                            {
                                string match = suggestions[0].term;
                                int score = getStringSim(this.keyword, match);
                                //Console.WriteLine("Replacing with " + match+ " with a score of " + score);

                                if (score > bestScore)
                                {
                                    bestScore = score;
                                    closest = match;
                                    boundingbox = coordinateBox;
                                }

                                Console.WriteLine("STRING MATCH: " + match + " OF  " + text + " LOCATED AT: " + "(" + coordinateBox.X1 + "," + coordinateBox.Y1 + ")" + "(" + coordinateBox.X2 + "," + coordinateBox.Y2 + ")");
                            }
                            else
                            {
                                int score = getStringSim(this.keyword, text);
                                //Console.WriteLine("Replacing with " + text + " with a score of " + score);

                                if (score > bestScore)
                                {
                                    bestScore = score;
                                    closest = text;
                                    boundingbox = coordinateBox;
                                }
                            }
                        }
                        else
                        {
                            int score = getStringSim(this.keyword, text);
                            //Console.WriteLine("Replacing with " + text + " with a score of " + score);

                            if (score > bestScore)
                            {
                                bestScore = score;
                                closest = text;
                                boundingbox = coordinateBox;
                            }
                        }
                    }
                }
                while (iter.Next(level));
            }

            //string text = page.GetText();
            //page.GetIterator();
            //Console.WriteLine(page.GetText());
            return (ret, closest, (boundingbox.X2 + boundingbox.X1) / 2, (boundingbox.Y2 + boundingbox.Y1) / 2);
        }

        //Default Full OCR workflow
        public (string, string, int, int) performOcr()
        {
            Image<Bgr, byte> image = getImage();
            Image<Gray, Byte> grayImage = getGrayScale(image);
            grayImage = resizeImage(grayImage);
            grayImage = blurImage(grayImage);
            performThresholding(grayImage);
            saveImage(grayImage);
            var ocrReturn = ocr();

            Console.WriteLine(ocrReturn.Item1);
            Console.WriteLine("Found Word: " + ocrReturn.Item2);
            Console.WriteLine("Coordinates: " + ocrReturn.Item3 / this.imageScaler + " x " + ocrReturn.Item4 / this.imageScaler);

            return (ocrReturn.Item1, ocrReturn.Item2, ocrReturn.Item3 / this.imageScaler, ocrReturn.Item4 / this.imageScaler);

        }



    }
}