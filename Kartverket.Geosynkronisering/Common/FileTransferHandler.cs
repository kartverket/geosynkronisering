using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.Text;
using Ionic.Zip;
using System.Xml;
using System.Net;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Security;
using NLog;
using System.Runtime.InteropServices;


namespace Kartverket.Geosynkronisering.Common
{
    public class FileTransferHandler
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)
        string m_zipFile;
        string m_workingDirectory;
        string m_packageName;
        int m_totalfiles = 0;
        int m_CurrentFile = 0;
        string m_fullPackageName;

        public enum ftpStatus: byte
        {
            done,
            error,
            processing
        }

        protected class FtpState
        {
            private ManualResetEvent wait;
            private FtpWebRequest request;
            private string fileName;
            private Exception operationException = null;
            private ftpStatus m_status;
            private string m_statusDescription;

            public FtpState()
            {
                wait = new ManualResetEvent(false);
            }

            public ManualResetEvent OperationComplete
            {
                get { return wait; }
            }

            public FtpWebRequest Request
            {
                get { return request; }
                set { request = value; }
            }

            public string FileName
            {
                get { return fileName; }
                set { fileName = value; }
            }
            public Exception OperationException
            {
                get { return operationException; }
                set { operationException = value; }
            }
            public ftpStatus status
            {
                get { return m_status; }
                set { m_status = value; }
            }
            public string StatusDescription
            {
                get { return m_statusDescription; }
                set { m_statusDescription = value; }
            }
        }

        protected class AsynchronousFtpUpLoader
        {

            private string m_ftpServer;
            private string m_user;
            private string m_pwd;
            private bool m_usePassive = true;

            public AsynchronousFtpUpLoader(string ftpserver, string user, string password)
            {
                m_ftpServer = ftpserver;
                m_user = user;
                m_pwd = password;
            }

            public bool usePassive
            {
                get { return m_usePassive; }
                set { m_usePassive = value; }
            }

            // Command line arguments are two strings:
            // 1. The url that is the name of the file being uploaded to the server.
            // 2. The name of the file on the local machine.
            //
            public void UploadFileToFtpServer(string fileName, string user = null, string password = null)
            {
                // Create a Uri instance with the specified URI string.
                // If the URI is not correctly formed, the Uri constructor
                // will throw an exception.
                ManualResetEvent waitObject;
                string file;
                if (fileName.Contains(@"\")) file = fileName.Substring(fileName.LastIndexOf(@"\") + 1); else file = fileName;

                //Må definere full target, ftp://server/folder/file.txt
                //Feiler dersom ikke file.txt er med.
                Uri target = new Uri(string.Concat("ftp://", m_ftpServer, "/", file));

                FtpState state = new FtpState();
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(target);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.UsePassive = m_usePassive;
                request.UseBinary = true;
                request.Proxy = null;


                // This example uses anonymous logon.
                // The request is anonymous by default; the credential does not have to be specified. 
                // The example specifies the credential only to
                // control how actions are logged on the server.
                if (user == null || password == null)
                {
                    request.Credentials = new NetworkCredential(m_user, m_pwd);
                }
                else
                {
                    request.Credentials = new NetworkCredential(user, password);
                }

                // Store the request in the object that we pass into the
                // asynchronous operations.
                state.Request = request;
                state.FileName = fileName;
                state.status = ftpStatus.processing;

                // Get the event to wait on.
                waitObject = state.OperationComplete;

                // Asynchronously get the stream for the file contents.
                request.BeginGetRequestStream(
                    new AsyncCallback(EndGetStreamCallback),
                    state
                );

                // Block the current thread until all operations are complete.
                waitObject.WaitOne();

                // The operations either completed or threw an exception.
                if (state.OperationException != null)
                {
                    throw state.OperationException;
                }
                else
                {
                    state.status = ftpStatus.done;
                    OnFileProgressChanged(new ProgressEventArgs(state.FileName, 1, 1, string.Format("The operation completed - {0}", state.StatusDescription), state.status, state.status == ftpStatus.error));
                }
            }

            public void downloadFileFromFTPServer(string SourceFileName, string TargetFilename, string user = null, string password = null)
            {
                // Create a Uri instance with the specified URI string.
                // If the URI is not correctly formed, the Uri constructor
                // will throw an exception.
                ManualResetEvent waitObject;
                string file;
                if (SourceFileName.Contains(@"\")) file = SourceFileName.Substring(SourceFileName.LastIndexOf(@"\") + 1); else file = SourceFileName;

                //Må definere full target, ftp://server/folder/file.txt
                //Feiler dersom ikke file.txt er med.
                Uri target = new Uri(string.Concat("ftp://", m_ftpServer, "/", file));

                FtpState state = new FtpState();
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(target);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.UsePassive = m_usePassive;
                request.UseBinary = true;
                request.Proxy = null;




                // This example uses anonymous logon.
                // The request is anonymous by default; the credential does not have to be specified. 
                // The example specifies the credential only to
                // control how actions are logged on the server.
                if (user == null || password == null)
                {
                    request.Credentials = new NetworkCredential(m_user, m_pwd);
                }
                else
                {
                    request.Credentials = new NetworkCredential(user, password);
                }

                // Store the request in the object that we pass into the
                // asynchronous operations.
                state.Request = request;
                state.FileName = TargetFilename;
                state.status = ftpStatus.processing;

                // Get the event to wait on.
                waitObject = state.OperationComplete;


                // Asynchronously get the stream for the file contents.
                request.BeginGetResponse(
                    new AsyncCallback(EndGetStreamCallbackDownload),
                    state
                );

                // Block the current thread until all operations are complete.                
                waitObject.WaitOne();

                // The operations either completed or threw an exception.
                if (state.OperationException != null)
                {
                    throw state.OperationException;
                }
                else
                {
                    OnFileProgressChanged(new ProgressEventArgs(state.FileName, 1, 1, string.Format("The operation completed - {0}", state.StatusDescription), state.status));
                }
            }

            private void EndGetStreamCallbackDownload(IAsyncResult ar)
            {
                FtpState state = (FtpState)ar.AsyncState;

                FtpWebResponse fileFtpRepsonse;
                Stream requestStream = null;
                FileStream stream = null;
                // End the asynchronous call to get the request stream.
                try
                {
                    fileFtpRepsonse = (FtpWebResponse)state.Request.EndGetResponse(ar);

                    requestStream = fileFtpRepsonse.GetResponseStream();
                    // Copy the file contents to the request stream.
                    const int bufferLength = 2048;
                    byte[] buffer = new byte[bufferLength];
                    int count = 0;
                    int readBytes = 2048;
                    stream = File.Create(state.FileName);
                    do
                    {
                        readBytes = requestStream.Read(buffer, 0, bufferLength);
                        stream.Write(buffer, 0, readBytes);
                        count += readBytes;
                        OnFileProgressChanged(new ProgressEventArgs(state.FileName, 1, 1, string.Format("Writing {0} bytes to the stream.", count.ToString()), state.status));
                    }
                    while (readBytes != 0);
                    // IMPORTANT: Close the request stream before sending the request.
                    requestStream.Close();
                    // Asynchronously get the response to the upload request.
                    state.Request.BeginGetResponse(
                        new AsyncCallback(EndGetResponseCallback),
                        state
                    );
                }
                // Return exceptions to the main application thread.
                catch (Exception e)
                {
                    state.status = ftpStatus.error;
                    OnFileProgressChanged(new ProgressEventArgs(state.FileName, 1, 1, string.Format("Could not get the request stream. \n{0}", e.Message.ToString()),state.status, true));
                    state.OperationException = e;
                    state.OperationComplete.Set();
                    return;
                }
                finally
                {
                    if (requestStream != null)
                    {
                        requestStream.Dispose();
                        requestStream = null;
                    }
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                        stream = null;
                    }                   
                }


            }
            private void EndGetStreamCallback(IAsyncResult ar)
            {
                FtpState state = (FtpState)ar.AsyncState;

                Stream requestStream = null;
                FileStream stream = null;
                // End the asynchronous call to get the request stream.
                try
                {
                    requestStream = state.Request.EndGetRequestStream(ar);
                    // Copy the file contents to the request stream.
                    const int bufferLength = 2048;
                    byte[] buffer = new byte[bufferLength];
                    int count = 0;
                    int readBytes = 0;
                    stream = File.OpenRead(state.FileName);
                    do
                    {
                        readBytes = stream.Read(buffer, 0, bufferLength);
                        requestStream.Write(buffer, 0, readBytes);
                        count += readBytes;
                        OnFileProgressChanged(new ProgressEventArgs(state.FileName, 1, 1, string.Format("Writing {0} bytes to the stream.", count.ToString()), state.status));
                    }
                    while (readBytes != 0);
                    // IMPORTANT: Close the request stream before sending the request.
                    requestStream.Close();
                    // Asynchronously get the response to the upload request.
                    state.Request.BeginGetResponse(
                        new AsyncCallback(EndGetResponseCallback),
                        state
                    );
                }
                // Return exceptions to the main application thread.
                catch (Exception e)
                {
                    state.status = ftpStatus.error;
                    OnFileProgressChanged(new ProgressEventArgs(state.FileName, 1, 1, string.Format("Could not get the request stream. \n{0}", e.Message.ToString()), state.status, true));
                    state.OperationException = e;
                    state.OperationComplete.Set();                   
                    return;
                }
                finally
                {
                    if (requestStream != null)
                    {
                        requestStream.Dispose();
                        requestStream = null;
                    }
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                        stream = null;
                    }
                }


            }

            // The EndGetResponseCallback method  
            // completes a call to BeginGetResponse.
            private void EndGetResponseCallback(IAsyncResult ar)
            {
                FtpState state = (FtpState)ar.AsyncState;
                FtpWebResponse response = null;
                try
                {
                    response = (FtpWebResponse)state.Request.EndGetResponse(ar);
                    response.Close();
                    state.StatusDescription = response.StatusDescription;
                    // Signal the main application thread that 
                    // the operation is complete.
                    state.OperationComplete.Set();                                       
                }
                // Return exceptions to the main application thread.
                catch (Exception e)
                {                   
                    state.OperationException = e;
                    state.OperationComplete.Set();
                    state.status = ftpStatus.error;                                
                }
            }




            public bool DeleteFileOnServer(string fileName, string user = null, string password = null)
            {
                // The serverUri parameter should use the ftp:// scheme.
                // It contains the name of the server file that is to be deleted.
                // Example: ftp://contoso.com/someFile.txt.
                // 
                
                string file;
                if (fileName.Contains(@"\")) file = fileName.Substring(fileName.LastIndexOf(@"\") + 1); else file = fileName;
                FtpState state = new FtpState();
                state.status = ftpStatus.processing;
                OnFileProgressChanged(new ProgressEventArgs(fileName, 1, 1, string.Format("Delete status: {0}{1}", "Starts deleting file:", file), state.status));

                Uri fullFilePath = new Uri(string.Concat("ftp://", m_ftpServer, "/", file));
                if (fullFilePath.Scheme != Uri.UriSchemeFtp)
                {
                    return false;
                }
                // Get the object used to communicate with the server.               
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fullFilePath);
                request.Method = WebRequestMethods.Ftp.DeleteFile;



                request.UsePassive = m_usePassive;
                request.UseBinary = true;
                request.Proxy = null;




                // This example uses anonymous logon.
                // The request is anonymous by default; the credential does not have to be specified. 
                // The example specifies the credential only to
                // control how actions are logged on the server.
                if (user == null || password == null)
                {
                    request.Credentials = new NetworkCredential(m_user, m_pwd);
                }
                else
                {
                    request.Credentials = new NetworkCredential(user, password);
                }


                FtpWebResponse response = null;
                try
                {
                    response = (FtpWebResponse)request.GetResponse();
                    state.status = ftpStatus.done;
                    OnFileProgressChanged(new ProgressEventArgs(fileName, 1, 1, string.Format("Delete status: {0}", response.StatusDescription), state.status));
                }
                catch (Exception ex)
                {
                    logger.InfoException(string.Format("Failed to delete file {0}.", fileName), ex);
                    return false;
                }
                finally
                {
                    if (response != null)
                    {
                        try
                        {
                            response.Close();
                        }
                        catch
                        {

                        }

                    }

                }
                return true;
            }

            private static IList<string> strFileList = new List<string>();

            public static bool GetFilListFromFTP(string ftpServer, string user, string pwd)
            {
                strFileList = new List<string>();
                FtpWebRequest fwr;
                try
                {
                    fwr = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServer + "//"));
                    fwr.Credentials = new NetworkCredential(user, pwd);
                    fwr.Method = WebRequestMethods.Ftp.ListDirectory;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                StreamReader sr = null;
                try
                {

                    sr = new StreamReader(fwr.GetResponse().GetResponseStream());

                    string str = sr.ReadLine();
                    if (str == null) return false;
                    while (str != null)
                    {
                        strFileList.Add(str);
                        str = sr.ReadLine();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (sr != null) sr.Close();
                    sr = null;
                    fwr = null;
                }


                return true;
            }

      

            public bool FileExistsOnServer(string fileName)
            {
                // The serverUri parameter should use the ftp:// scheme.
                // It contains the name of the server file that is to be deleted.
                // Example: ftp://contoso.com/someFile.txt.
                // 

                string file;
                if (fileName.Contains(@"\")) file = fileName.Substring(fileName.LastIndexOf(@"\") + 1); else file = fileName;
                OnFileProgressChanged(new ProgressEventArgs(fileName, 1, 1, string.Format("Check if file exists: {0}{1}", "Looking for file:", file), ftpStatus.processing));

                Uri fullFilePath = new Uri(string.Concat("ftp://", m_ftpServer, "/", file));
                if (fullFilePath.Scheme != Uri.UriSchemeFtp)
                {
                    return false;
                }
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fullFilePath);
                request.Credentials = new NetworkCredential(m_user, m_pwd);
                request.Method = WebRequestMethods.Ftp.GetDateTimestamp;

                FtpWebResponse response = null;
                try
                {
                    response = (FtpWebResponse)request.GetResponse();
                    OnFileProgressChanged(new ProgressEventArgs(fileName, 1, 1, string.Format("Check file status: {0}", response.StatusDescription),ftpStatus.processing));
                }
                catch (WebException ex)
                {
                    FtpWebResponse responseRes = (FtpWebResponse)ex.Response;
                    if (responseRes.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    logger.InfoException(string.Format("Could not list files on FTP server {0}.", m_ftpServer), ex);
                    return false;
                }
                finally
                {
                    if (response != null) response.Close();
                }
                return true;
            }

            public delegate void FileProgressHandler(object sender, ProgressEventArgs e);

            public event FileProgressHandler FileProgressChanged;

            protected virtual void OnFileProgressChanged(ProgressEventArgs e)
            {
                if (FileProgressChanged != null)
                {
                    FileProgressChanged(this, e);
                }
            }

        }



        public bool UploadFileToFtp(string FileName, string ftpserver, string user, string password)
        {
            m_Done = false;
            m_status = ftpStatus.processing;
            AsynchronousFtpUpLoader ftp = new AsynchronousFtpUpLoader(ftpserver, user, password);
            try
            {
                ftp.FileProgressChanged += new AsynchronousFtpUpLoader.FileProgressHandler(FileProgressChanged);                
                ftp.UploadFileToFtpServer(FileName, user, password);
                DateTime starttid = DateTime.Now;
                long elapsedTicks = DateTime.Now.Ticks - starttid.Ticks;

                TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
                int timeout = 5;

                while (!m_Done && elapsedSpan.Minutes < timeout)
                {
                   
                    System.Threading.Thread.Sleep(3000);
                    elapsedTicks = DateTime.Now.Ticks - starttid.Ticks;
                    elapsedSpan = new TimeSpan(elapsedTicks);
                }

            }
            catch (Exception ex)
            {
                logger.ErrorException(string.Format("Failed to upload file {0} to ftpserver {1}", FileName, ftpserver), ex);
                return false;
            }
            finally
            {
                ftp = null;
            }

            if (m_status == ftpStatus.error) return false;
            return m_Done;
        }


        public bool DownloadFileFromFtp(string localFileName, string ftpFileName, string ftpserver, string user, string password)
        {

            m_Done = false;
            AsynchronousFtpUpLoader ftp = new AsynchronousFtpUpLoader(ftpserver, user, password);
          
            try
            {
                ftp.FileProgressChanged += new AsynchronousFtpUpLoader.FileProgressHandler(FileProgressChanged);
                if (!ftp.FileExistsOnServer(ftpFileName)) throw new Exception(string.Format("File {0} is not found on FTP server {1}.", ftpFileName, ftpserver));
                
                ftp.downloadFileFromFTPServer(ftpFileName, localFileName);
            }
            catch (Exception ex)
            {
                string preMessage = string.Format("Error downloading file: {0} from ftp server {1} to the location:{2}.", ftpFileName, ftpserver, localFileName);
                logger.ErrorException(preMessage, ex);
                throw new Exception(preMessage, ex);
            }
            finally
            {
                ftp = null;
            }
            return true;
     

        }


        public bool CleanUpFTP(string fileName, string ftpserver, string user, string pwd)
        {
            m_Done = false;
            AsynchronousFtpUpLoader ftp = new AsynchronousFtpUpLoader(ftpserver, user, pwd);
            try
            {
                ftp.FileProgressChanged += new AsynchronousFtpUpLoader.FileProgressHandler(FileProgressChanged);
                ftp.DeleteFileOnServer(fileName);
            }
            catch (Exception ex)
            {
                string preMessage = string.Format("Failed deleting file: {0} from ftp server {1}.", fileName, ftpserver);
                logger.ErrorException(preMessage, ex);
                return false;
            }
            finally
            {
                ftp = null;
            }
            return true;
        }

     

        public delegate void ProgressHandler(object sender, ProgressEventArgs e);

        public event ProgressHandler ProgressChanged;

        public delegate void ProcessDoneHandler(object sender, ProgressEventArgs e);

        public event ProcessDoneHandler ProcessDone;

    
        protected virtual void OnProgressChanged(ProgressEventArgs e)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, e);
            }
        }

        protected virtual void OnProcressDone(ProgressEventArgs e)
        {
            if (ProcessDone != null)
            {
                ProcessDone(this, e);
            }
        }


        private bool m_Done = false;
        private ftpStatus m_status = ftpStatus.processing;

        private void FileProgressChanged(object sender, ProgressEventArgs e)
        {
            OnProgressChanged(e);
            if (e.status == ftpStatus.done || e.status == ftpStatus.error)
            {
                m_Done = true;
                m_status = e.status;
                OnProcressDone(e);               
            }
            else m_Done = false;
        }

  
        public class ProgressEventArgs : EventArgs
        {
            private string m_fileName = "";
            public string fileName
            {
                get
                {
                    return m_fileName;
                }
            }

            private int m_totalfiles;
            public int totalFiles
            {
                get
                {
                    return m_totalfiles;
                }
            }

            private int m_currentFile;
            public int currentFile
            {
                get
                {
                    return m_currentFile;
                }
            }

            private string m_errorMessage;
            public string errorMessage
            { get { return m_errorMessage; } }

            private bool m_error;
            public bool error
            {
                get
                {
                    return m_error;
                }
            }

            private ftpStatus m_status;
            public ftpStatus status
            { get { return m_status; } }

            public ProgressEventArgs(string FileName, int TotalFiles, int CurrentFile, string errorMsg, ftpStatus status, bool Error = false)
            {
                m_fileName = FileName;
                m_totalfiles = TotalFiles;
                m_currentFile = CurrentFile;
                m_errorMessage = errorMsg;
                m_status = status;

                m_error = Error;
            }

        }

      

    }

}







  
  
        