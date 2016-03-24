

using Leadtools;
using Leadtools.Documents;
using Leadtools.Documents.Converters;
using Leadtools.Documents.UI;
using Leadtools.Forms.Ocr;
using System;
using System.IO;
using System.Windows.Forms;


namespace LeadToolsPhotoTextRecognition
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                SetLicense();
                InitDocumentViewer();
                InitOcrEngine();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            base.OnLoad(e);
        }

        private void SetLicense()
        {
            RasterSupport.SetLicense(Application.StartupPath + @"\license\eval-license-files.lic",
                File.ReadAllText(Application.StartupPath + @"\license\eval-license-files.lic.key").Trim());
        }

        DocumentViewer _documentViewer = null;

        private void InitDocumentViewer()
        {
            // Create the document viewer using panels of a System.Windows.Forms.SplitterPanel
            var createOptions = new Leadtools.Documents.UI.DocumentViewerCreateOptions();
            createOptions.ViewContainer = this.splitContainer1.Panel2;
            createOptions.ThumbnailsContainer = this.splitContainer1.Panel1;
            _documentViewer = DocumentViewerFactory.CreateDocumentViewer(createOptions);
            _documentViewer.View.PreferredItemType = DocumentViewerItemType.Svg;
            _documentViewer.Commands.Run(DocumentViewerCommands.InteractivePanZoom);
            _documentViewer.Text.AutoGetText = true;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = "c:\\";
            fileDialog.Filter = "jpg files (*.jpg)|*.jpg|All files (*.*)|*.*";
            fileDialog.RestoreDirectory = true;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadDocument(fileDialog.FileName);

            }
        }

        private void LoadDocument(string path)
        {
            var document = DocumentFactory.LoadFromFile(
                    path,
                    new LoadDocumentOptions { UseCache = false });
            document.Text.OcrEngine = _ocrEngine;
            // Set in the viewer
            _documentViewer.SetDocument(document);

        }

        private IOcrEngine _ocrEngine = null;
        private void InitOcrEngine()
        {
            try
            {
                _ocrEngine = OcrEngineManager.CreateEngine(OcrEngineType.Advantage, false);
                _ocrEngine.Startup(null, null, null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Failed to start the Advantage OCR engine. The demo will continue running without OCR functionality and you will not be able to parse text from non-document files such as TIFF or Raster PDF.\n\nError message:\n{0}", ex.Message));
            }
        }

        private void getTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var thisOperation = new DocumentViewerAsyncOperation
            {
                Error = (DocumentViewerAsyncOperation operation, Exception error) =>
                {
                    MessageBox.Show(error.Message);
                },
                Always = (DocumentViewerAsyncOperation operation) =>
                {
                    MessageBox.Show("DONE!");
                }
            };

            _documentViewer.Commands.RunAsync(thisOperation, DocumentViewerCommands.TextGet, _documentViewer.CurrentPageNumber);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var converter = new DocumentConverter();
            converter.SetOcrEngineInstance(_ocrEngine, false);
           
            var jobData = new DocumentConverterJobData() {
                Document = _documentViewer.Document,
                DocumentFormat = Leadtools.Forms.DocumentWriters.DocumentFormat.Docx,
                JobName = "SaveToDocx",
                OutputDocumentFileName = "sample.docx"
            };

            var job = converter.Jobs.CreateJob(jobData);

            converter.Jobs.RunJob(job);

            if(job.Status== DocumentConverterJobStatus.Success)
            {
                MessageBox.Show("Word Document Created");
            } else
            {
                MessageBox.Show("Word Document Creation Failed");
            }
        }

        private void selectTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            _documentViewer.Commands.Run(DocumentViewerCommands.InteractiveSelectText);
        }

        private void copyTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _documentViewer.Commands.Run(DocumentViewerCommands.TextCopy);
        }
    }
}
