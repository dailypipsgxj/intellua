﻿#region Using Directives

using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using ScintillaNET;
using WeifenLuo.WinFormsUI.Docking;

#endregion Using Directives


namespace IntelluaTE
{
    internal sealed partial class DocumentForm : DockContent
    {
        #region Fields

        // Indicates that calls to the StyleNeeded event
        // should use the custom INI lexer
        private bool _iniLexer;
        public MainForm m_mainForm;
        #endregion Fields


        #region Methods

        private void AddOrRemoveAsteric()
        {
            if (scintilla.Modified)
            {
                if (!Text.EndsWith(" *"))
                    Text += " *";
            }
            else
            {
                if (Text.EndsWith(" *"))
                    Text = Text.Substring(0, Text.Length - 2);
            }
        }


        private void DocumentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Scintilla.Modified)
            {
                // Prompt if not saved
                string message = String.Format(
                    CultureInfo.CurrentCulture,
                    "The _text in the {0} file has changed.{1}{2}Do you want to save the changes?",
                    Text.TrimEnd(' ', '*'),
                    Environment.NewLine,
                    Environment.NewLine);

                DialogResult dr = MessageBox.Show(this, message, Program.Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                if (dr == DialogResult.Cancel)
                {
                    // Stop closing
                    e.Cancel = true;
                    return;
                }
                else if (dr == DialogResult.Yes)
                {
                    // Try to save before closing
                    e.Cancel = !Save();
                    return;
                }
            }

            // Close as normal
        }


        public bool ExportAsHtml()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                string fileName = (Text.EndsWith(" *") ? Text.Substring(0, Text.Length - 2) : Text);
                dialog.Filter = "HTML Files (*.html;*.htm)|*.html;*.htm|All Files (*.*)|*.*";
                dialog.FileName = fileName + ".html";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    scintilla.Lexing.Colorize(); // Make sure the document is current
                    using (StreamWriter sw = new StreamWriter(dialog.FileName))
                        scintilla.ExportHtml(sw, fileName, false);

                    return true;
                }
            }

            return false;
        }


        public bool Save()
        {
            if (String.IsNullOrEmpty(FilePath))
                return SaveAs();

            return Save(FilePath);
        }


        public bool Save(string filePath)
        {
            using (FileStream fs = File.Create(filePath))
            using (BinaryWriter bw = new BinaryWriter(fs))
                bw.Write(scintilla.RawText, 0, scintilla.RawText.Length - 1); // Omit trailing NULL

            scintilla.Modified = false;

            Text = Path.GetFileName(filePath);
            return true;
        }


        public bool SaveAs()
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                FilePath = saveFileDialog.FileName;
                return Save(FilePath);
            }

            return false;
        }


        private void scintilla_ModifiedChanged(object sender, EventArgs e)
        {
            AddOrRemoveAsteric();
        }


        #endregion Methods


        #region Properties

        public string FilePath
        {
            get { return scintilla.FilePath; }
            set { scintilla.FilePath = value;}
        }


        public bool IniLexer
        {
            get { return _iniLexer; }
            set { _iniLexer = value; }
        }


        public Scintilla Scintilla
        {
            get
            {
                return scintilla;
            }
        }

        #endregion Properties


        #region Constructors

        public DocumentForm()
        {
           
            InitializeComponent();
            
            scintilla.setParent(Program.data);

        }

        #endregion Constructors

        public void ReloadClassDef(){
            scintilla.setParent(Program.data);
        }

        private void DocumentForm_Load(object sender, EventArgs e)
        {
           
        }

        public void ParseFile() {
            scintilla.queueParseFile();
        }

        private void scintilla_StatusChanged(object sender, Intellua.StatusChangedEventArgs e)
        {
            m_mainForm.setStatusText(e.Text);
        }
    }
}
