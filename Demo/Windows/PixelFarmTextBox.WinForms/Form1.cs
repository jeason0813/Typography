﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Text;
using System.Windows.Forms;


using PixelFarm.Agg;
using PixelFarm.Drawing.Fonts;

using Typography.OpenFont;
using Typography.Rendering;
using Typography.Contours;

using SampleWinForms;


namespace PixelFarmTextBox.WinForms
{
    public partial class Form1 : Form
    {
        Graphics g;
        AggCanvasPainter painter;
        ImageGraphics2D imgGfx2d;
        ActualImage destImg;
        Bitmap winBmp;

        DevTextPrinterBase selectedTextPrinter = null;
        VxsTextPrinter _devVxsTextPrinter = null;
        SampleWinForms.UI.SampleTextBoxControllerForPixelFarm _controllerForPixelFarm = new SampleWinForms.UI.SampleTextBoxControllerForPixelFarm();

        TypographyTest.BasicFontOptions _basicOptions;
        TypographyTest.GlyphRenderOptions _renderOptions;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;
            //
            _basicOptions = basicFontOptionsUserControl1.Options;
            _basicOptions.TypefaceChanged += (s, e2) =>
            {
                if (_devVxsTextPrinter != null)
                {
                    _devVxsTextPrinter.Typeface = e2.SelectedTypeface;
                }
            };
            _basicOptions.UpdateRenderOutput += (s, e2) =>
             {
                 UpdateRenderOutput();
             };
            //
            //---------- 
            _renderOptions = glyphRenderOptionsUserControl1.Options;
            _renderOptions.UpdateRenderOutput += (s, e2) =>
            {
                UpdateRenderOutput();
            };

            //share text printer to our sample textbox
            //but you can create another text printer that specific to text textbox control

            destImg = new ActualImage(800, 600, PixelFormat.ARGB32);
            imgGfx2d = new ImageGraphics2D(destImg); //no platform
            painter = new AggCanvasPainter(imgGfx2d);
            winBmp = new Bitmap(destImg.Width, destImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            g = this.sampleTextBox1.CreateGraphics();

            painter.CurrentFont = new PixelFarm.Drawing.RequestFont("tahoma", 14);

            _devVxsTextPrinter = new VxsTextPrinter(painter, _basicOptions.OpenFontStore);
            _devVxsTextPrinter.TargetCanvasPainter = painter;
            _devVxsTextPrinter.ScriptLang = _basicOptions.ScriptLang;
            _devVxsTextPrinter.PositionTechnique = Typography.TextLayout.PositionTechnique.OpenFont;
            _devVxsTextPrinter.FontSizeInPoints = 10;

            _controllerForPixelFarm.BindHostGraphics(g);
            _controllerForPixelFarm.TextPrinter = _devVxsTextPrinter;

            this.sampleTextBox1.SetController(_controllerForPixelFarm);
            _readyToRender = true;
            _basicOptions.UpdateRenderOutput += (s, e2) => UpdateRenderOutput();
            //----------
            //txtInputChar.TextChanged += (s, e2) => UpdateRenderOutput();
            //----------
        }
        bool _readyToRender;
        void UpdateRenderOutput()
        {
            if (!_readyToRender) return;
            //

            //test option use be used with lcd subpixel rendering.
            //this demonstrate how we shift a pixel for subpixel rendering tech
            _devVxsTextPrinter.UseWithLcdSubPixelRenderingTechnique = true;

            //1. read typeface from font file 
            TypographyTest.RenderChoice renderChoice = _basicOptions.RenderChoice;
            switch (renderChoice)
            {

                case TypographyTest.RenderChoice.RenderWithGdiPlusPath:
                    //not render in this example
                    //see more at ...
                    break;
                case TypographyTest.RenderChoice.RenderWithTextPrinterAndMiniAgg:
                    {
                        //clear previous draw
                        painter.Clear(PixelFarm.Drawing.Color.White);
                        painter.UseSubPixelRendering = false;
                        painter.FillColor = PixelFarm.Drawing.Color.Black;

                        selectedTextPrinter = _devVxsTextPrinter;
                        selectedTextPrinter.Typeface = _basicOptions.Typeface;
                        selectedTextPrinter.FontSizeInPoints = _basicOptions.FontSizeInPoints;
                        selectedTextPrinter.ScriptLang = _basicOptions.ScriptLang;
                        selectedTextPrinter.PositionTechnique = _basicOptions.PositionTech;

                        selectedTextPrinter.HintTechnique = HintTechnique.None;
                        selectedTextPrinter.EnableLigature = true;
                        _devVxsTextPrinter.UpdateGlyphLayoutSettings();
                        _controllerForPixelFarm.ReadyToRender = true;


                        _controllerForPixelFarm.UpdateOutput();

                        //copy from Agg's memory buffer to gdi 
                        PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
                        g.Clear(Color.White);
                        g.DrawImage(winBmp, new Point(10, 0));

                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
