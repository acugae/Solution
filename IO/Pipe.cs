namespace Solution.IO;

public delegate void DelegateMessage(string Reply);
class PipeServer
{
    string _pipeName;
    NamedPipeServerStream pipeServer = null;

    public void Create(string PipeName)
    {
        _pipeName = PipeName;
        pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous | PipeOptions.FirstPipeInstance);
    }

    public void WriteLine(string sMessage)
    {
        try
        {
            if (pipeServer == null || !pipeServer.IsConnected)
                return;
            using (StreamWriter sw = new StreamWriter(pipeServer, Encoding.UTF8, 1024, leaveOpen: true))
            {
                sw.AutoFlush = true;
                sw.WriteLine(sMessage);
            }
        }
        catch (Exception oEX)
        {
            Console.WriteLine(oEX.Message);
        }
    }

    public void Close(string PipeName)
    {
        // Kill original sever and create new wait server
        pipeServer?.Close();
        pipeServer = null;
    }
}

class PipeClient
{
    NamedPipeClientStream pipeStream = null;
    int iTimeout = 1000;
    //
    public void Load(string PipeName, int TimeOut = 1000)
    {
        pipeStream = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        iTimeout = TimeOut;
    }
    //
    public string Receive()
    {
        try
        {
            if (pipeStream == null)
                return null;
            if (!pipeStream.IsConnected)
                pipeStream.Connect(iTimeout);
            //
            using (StreamReader sr = new StreamReader(pipeStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
            {
                string temp;
                while ((temp = sr.ReadLine()) != null)
                {
                    return temp;
                }
            }
        }
        catch (TimeoutException)
        {
            return null;
        }
        return null;
    }
    //
    //private void AsyncSend(IAsyncResult iar)
    //{
    //    try
    //    {
    //        // Get the pipe
    //        NamedPipeClientStream pipeStream = (NamedPipeClientStream)iar.AsyncState;
    //        // End the write
    //        pipeStream.EndWrite(iar);
    //        pipeStream.Flush();
    //        pipeStream.Close();
    //        pipeStream.Dispose();
    //    }
    //    catch (Exception oEX)
    //    {
    //        Console.WriteLine(oEX.Message);
    //    }
    //}
}
