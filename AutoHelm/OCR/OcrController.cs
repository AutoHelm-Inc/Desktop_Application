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

namespace Ocr
{
    public class OcrController
    {

        //Qualitative Member Variables
        private int controllerId = 0;
        private string file;
        private int imageScaler = 3;
        private int kernelSmoothingSize = 1;
        private int sigmaSmoothingSize = 0;
        private int threshBlockSize = 31;
        private double threshC = 0.03;
        private static int numControllers = 0;
        private String whiteList = "qwertyuioplkjhgfdsazxcvbnmQWERTYUIOPLKJHGFDSAZXCVBNM.,?!()\n\t ";
        private bool shouldStringMatch = false;

        //Quantitative Member Variables
        private Emgu.CV.CvEnum.Inter resizeMode = Emgu.CV.CvEnum.Inter.Cubic;
        private Emgu.CV.CvEnum.AdaptiveThresholdType threshType = Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC;
        private bool isDarkMode = false;

        //Different Constructors For Different Scenarios
        //This constructor assumes default parameters 
        public OcrController(string filePath, bool darkMode)
        {
            this.file = filePath;
            this.isDarkMode = darkMode;
            this.controllerId = numControllers;
            this.shouldStringMatch = true;
            numControllers++;
        }

        //Constructor with custom image scaling
        public OcrController(string filePath, int scale, Emgu.CV.CvEnum.Inter mode, bool darkMode, bool sm)
        {
            this.file = filePath;
            this.imageScaler = scale;
            this.resizeMode = mode;
            this.isDarkMode = darkMode;
            this.controllerId = numControllers;
            this.shouldStringMatch = sm;
            numControllers++;
        }

        //Constructor with custom smoothing
        public OcrController(string filePath, int kernelSize, int sigmaSize, bool darkMode, bool sm)
        {
            this.file = filePath;
            this.kernelSmoothingSize = kernelSize;
            this.sigmaSmoothingSize = sigmaSize;
            this.isDarkMode = darkMode;
            this.controllerId = numControllers;
            this.shouldStringMatch = sm;
            numControllers++;
        }

        //Constructor for custom thresholding
        public OcrController(string filePath, int blockSize, int c, Emgu.CV.CvEnum.AdaptiveThresholdType threshMode, bool darkMode, bool sm)
        {
            this.file = filePath;
            this.threshBlockSize = blockSize;
            this.threshC = c;
            this.threshType = threshMode;
            this.isDarkMode = darkMode;
            this.shouldStringMatch = sm;
            this.controllerId = numControllers;
            numControllers++;
        }

        public OcrController(string filePath, int scale, int kernelSize, int sigmaSize, Emgu.CV.CvEnum.Inter sizeMode, int blockSize, int c, Emgu.CV.CvEnum.AdaptiveThresholdType threshMode, bool darkMode, bool sm)
        {
            this.file = filePath;
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
        private Image<Bgr, byte>  getImage()
        {
            return new Image<Bgr, byte>(file);
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

        //Function to Perform OCR
        //TODO: look for special characters
        //TODO: string matching
        private string ocr()
        {
            var img = Pix.LoadFromFile(System.AppDomain.CurrentDomain.BaseDirectory + "\\" + "Grey" + "_" + controllerId.ToString() + ".PNG");
            var ocr = new TesseractEngine(System.AppDomain.CurrentDomain.BaseDirectory + "\\ocrResources", "eng", EngineMode.Default);
            ocr.SetVariable("tessedit_char_whitelist", whiteList);
            var page = ocr.Process(img);
            string text = page.GetText();
            Console.WriteLine(page.GetText());
            return text;
        }

        //Default Full OCR workflow
        public string performOcr()
        {
            Image<Bgr, byte> image = getImage();
            Image<Gray, Byte> grayImage = getGrayScale(image);
            grayImage = resizeImage(grayImage);
            grayImage = blurImage(grayImage);
            performThresholding(grayImage);
            saveImage(grayImage);
            string text = ocr();
            string stringMatchingRet = performStringMatching(text);

            if (stringMatchingRet.Length > 0)
            {
                return stringMatchingRet;
            }
            else
            {
                return text;
            }

        }



    }
}