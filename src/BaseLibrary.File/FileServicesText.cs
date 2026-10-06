using System.Text;

namespace BaseLibrary;

public class FileServicesText : IFileServicesText
{
    public bool WriteTXT(string pathFile, string parTXT)
    {
        if (string.IsNullOrWhiteSpace(pathFile))
            throw new ArgumentNullException(nameof(pathFile));
        if (parTXT is null)
            throw new ArgumentNullException(nameof(parTXT));

        var dir = Path.GetDirectoryName(pathFile);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            return false;
        }

        short count = 0;
        bool isCont = true;
        bool returnBak = false;

        bool isnew = !File.Exists(pathFile);
        string newPath = pathFile + newExt;

        try
        {
            BakWriteBegin(pathFile);
            while (isCont && count < 3)
            {
                count++;
                isCont = false;
                try
                {
                    // Escrita atomica: grava o novo conteudo num arquivo temporario (.new)
                    // e so entao o promove ao arquivo final por File.Replace/Move (commit
                    // atomico). Assim uma falha no meio da escrita nunca trunca o arquivo
                    // final, que so muda pela operacao de commit (fica com o conteudo antigo
                    // ou com o novo, nunca parcial).
                    WriteContentToNewFile(newPath, parTXT);
                    CommitNewFile(newPath, pathFile);
                    DateTime now = DateTime.Now;
                    if (isnew)
                    {
                        File.SetCreationTime(pathFile, now);
                    }
                    File.SetLastWriteTime(pathFile, now);
                    return true;
                }
                catch (IOException)
                {
                    // A escrita falhou no meio: descarta o .new parcial. O arquivo final
                    // continua integro com o conteudo anterior.
                    SafeDeleteNewFile(pathFile);
                    isCont = true;
                    Thread.Sleep(100);
                    if (!returnBak)
                        returnBak = true;
                }
                catch (Exception)
                {
                    SafeDeleteNewFile(pathFile);
                    if (!returnBak)
                        returnBak = true;
                }
            }
        }
        finally
        {
            SafeDeleteNewFile(pathFile);
            BakWriteEnd(pathFile, returnBak);
        }

#if DEBUG
        if (File.Exists(pathFile + tmpExt))
        {

        }
#endif

        return false;
    }
    public async Task<bool> WriteTXTAsync(string pathFile, string parTXT)
    {
        if (string.IsNullOrWhiteSpace(pathFile))
            throw new ArgumentNullException(nameof(pathFile));
        if (parTXT is null)
            throw new ArgumentNullException(nameof(parTXT));

        var dirAsync = Path.GetDirectoryName(pathFile);
        if (!string.IsNullOrEmpty(dirAsync) && !Directory.Exists(dirAsync))
        {
            return false;
        }

        short count = 0;
        bool isCont = true;
        bool returnBak = false;

        bool isnew = !File.Exists(pathFile);
        string newPath = pathFile + newExt;

        try
        {
            BakWriteBegin(pathFile);
            while (isCont && count < 10)
            {
                count++;
                isCont = false;
                try
                {
                    // Escrita atomica: mesma estrategia do WriteTXT sincrono. O conteudo
                    // e escrito de forma assincrona no arquivo temporario (.new) e so
                    // entao promovido ao arquivo final pelo commit atomico.
                    await WriteContentToNewFileAsync(newPath, parTXT);
                    CommitNewFile(newPath, pathFile);
                    DateTime now = DateTime.Now;
                    if (isnew)
                    {
                        File.SetCreationTime(pathFile, now);
                    }
                    File.SetLastWriteTime(pathFile, now);
                    return true;
                }
                catch (IOException)
                {
                    SafeDeleteNewFile(pathFile);
                    isCont = true;
                    await Task.Delay(200);
                    if (!returnBak)
                        returnBak = true;
                }
                catch (Exception)
                {
                    SafeDeleteNewFile(pathFile);
                    if (!returnBak)
                        returnBak = true;
                }
            }
        }
        finally
        {
            SafeDeleteNewFile(pathFile);
            BakWriteEnd(pathFile, returnBak);
        }

#if DEBUG
        if (File.Exists(pathFile + tmpExt))
        {

        }
#endif

        return false;
    }
    private const string tmpExt = ".tmp";
    // Extensao do arquivo temporario da escrita atomica: o novo conteudo e gravado
    // aqui e so depois promovido ao arquivo final por File.Replace/Move (CommitNewFile).
    private const string newExt = ".new";
    // Escreve o novo conteudo no arquivo temporario (.new). protected virtual para
    // permitir que testes de regressao simulem uma falha no meio da escrita (ex.: disco
    // cheio) e verifiquem que o arquivo final permanece integro.
    protected virtual void WriteContentToNewFile(string newPath, string content)
    {
        using StreamWriter sw = new(newPath, false, Encoding.UTF8);
        sw.Write(content);
    }
    // Variante assincrona do seam de escrita, usada por WriteTXTAsync.
    protected virtual async Task WriteContentToNewFileAsync(string newPath, string content)
    {
        using StreamWriter sw = new(newPath, false, Encoding.UTF8);
        await sw.WriteAsync(content);
    }
    // Commit atomico: promove o arquivo temporario a arquivo final numa unica operacao
    // do sistema de arquivos. Antes disso o final permanece com o conteudo anterior;
    // depois, com o novo. Nunca fica num estado intermediario. protected virtual para
    // permitir que os testes simulem uma falha exatamente na promocao.
    protected virtual void CommitNewFile(string newPath, string finalPath)
    {
        if (File.Exists(finalPath))
        {
            try
            {
                // File.Replace e transacional no NTFS e preserva os atributos do destino.
                File.Replace(newPath, finalPath, null);
            }
            catch (Exception) when (File.Exists(newPath))
            {
                // Fallback para sistemas de arquivo sem suporte a ReplaceFile (ex.: exFAT):
                // File.Move com overwrite tambem e atomico no mesmo volume (MoveFileEx).
                File.Move(newPath, finalPath, true);
            }
        }
        else
        {
            File.Move(newPath, finalPath);
        }
    }
    // Remove o arquivo temporario (.new), se existir. Best-effort: nao deve mascarar o
    // erro original de uma escrita que falhou.
    private static void SafeDeleteNewFile(string pathFile)
    {
        string newPath = pathFile + newExt;
        if (File.Exists(newPath))
        {
            try { File.Delete(newPath); } catch { /* best effort */ }
        }
    }
    private void BakWriteBegin(string pathFile)
    {
        if (File.Exists(pathFile))
        {
            File.Copy(pathFile, pathFile + tmpExt, true);
            File.SetCreationTime(pathFile + tmpExt, DateTime.Now);
        }
    }
    private void BakWriteEnd(string pathFile, bool returnBak)
    {
        if (returnBak)
        {
            if (File.Exists(pathFile + tmpExt))
            {
                try
                {
                    DateTime creation = File.GetCreationTime(pathFile + tmpExt);
                    DateTime lastWrite = File.GetLastWriteTime(pathFile + tmpExt);
                    File.Copy(pathFile + tmpExt, pathFile, true);
#if DEBUG
                    var trash = File.GetCreationTime(pathFile);
                    var trash2 = File.GetLastWriteTime(pathFile);
#endif

                    if (File.GetCreationTime(pathFile) != creation)
                    {
                        File.SetCreationTime(pathFile, creation);
                    }
                    if (File.GetLastWriteTime(pathFile) != lastWrite)
                    {
                        File.SetLastWriteTime(pathFile, lastWrite);
                    }
                }
                catch (Exception)
                {
                    //TODO verificar o que fazer
                }
            }
        }
        if (File.Exists(pathFile + tmpExt) && File.Exists(pathFile) && File.GetLastWriteTime(pathFile) > File.GetCreationTime(pathFile + tmpExt))
        {
            try
            {
                File.Delete(pathFile + tmpExt);
            }
            catch (Exception)
            {

            }
        }
    }
    public string? ReadTXT(string pathFile)
    {
        if (string.IsNullOrWhiteSpace(pathFile))
            throw new ArgumentNullException(nameof(pathFile));


        short count = 0;
        bool isCont = true;
        string fileTemp = BakReadBegin(pathFile);
        if (fileTemp == pathFile + tmpExt)
        {
            var readDir = Path.GetDirectoryName(pathFile);
            if (!string.IsNullOrEmpty(readDir) && !Directory.Exists(readDir))
                return null;
        }
        try
        {
            while (isCont && count < 3)
            {
                count++;
                isCont = false;
                try
                {
                    using (StreamReader sr = new StreamReader(fileTemp))
                    {
                        return sr.ReadToEnd();
                    }
                }
                catch (IOException)
                {
                    isCont = true;
                    Thread.Sleep(100);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        finally
        {
            BakReadEnd(pathFile);
        }

#if DEBUG
        if (File.Exists(pathFile + tmpExt))
        {

        }
#endif

        return null;
    }
    public async Task<string?> ReadTXTAsync(string pathFile)
    {
        if (string.IsNullOrWhiteSpace(pathFile))
            throw new ArgumentNullException(nameof(pathFile));

        short count = 0;
        bool isCont = true;
        string fileTemp = BakReadBegin(pathFile);
        if (fileTemp == pathFile + tmpExt)
        {
            var readDir = Path.GetDirectoryName(pathFile);
            if (!string.IsNullOrEmpty(readDir) && !Directory.Exists(readDir))
                return null;
        }
        try
        {
            while (isCont && count < 10)
            {
                count++;
                isCont = false;
                try
                {
                    using (StreamReader sr = new StreamReader(fileTemp))
                    {
                        return await sr.ReadToEndAsync();
                    }
                }
                catch (IOException)
                {
                    isCont = true;
                    await Task.Delay(200);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        finally
        {
            BakReadEnd(pathFile);
        }

#if DEBUG
        if (File.Exists(pathFile + tmpExt))
        {

        }
#endif
        return null;
    }
    private string BakReadBegin(string pathFile)
    {

#if DEBUG
        if ((File.Exists(pathFile + tmpExt) && !File.Exists(pathFile)) || (File.Exists(pathFile + tmpExt) && File.Exists(pathFile) && File.GetCreationTime(pathFile + tmpExt) > File.GetCreationTime(pathFile)))
        {

        }
        var trash = File.GetLastWriteTime(pathFile + tmpExt);
        DateTime? trash2 = (File.Exists(pathFile) ? File.GetLastWriteTime(pathFile) : null);
        var trash3 = File.GetCreationTime(pathFile + tmpExt);
        DateTime? trash4 = (File.Exists(pathFile) ? File.GetCreationTime(pathFile) : null);
#endif

        if (File.Exists(pathFile + tmpExt) && (!File.Exists(pathFile) || File.GetCreationTime(pathFile + tmpExt) > File.GetLastWriteTime(pathFile)))
        {
            try
            {
                DateTime creation = File.GetCreationTime(pathFile + tmpExt);
                DateTime lastWrite = File.GetLastWriteTime(pathFile + tmpExt);

                File.Move(pathFile + tmpExt, pathFile, true);
#if DEBUG
                var trash10 = File.GetCreationTime(pathFile);
                var trash12 = File.GetLastWriteTime(pathFile);
#endif

                if (File.GetCreationTime(pathFile) != creation)
                {
                    File.SetCreationTime(pathFile, creation);
                }
                if (File.GetLastWriteTime(pathFile) != lastWrite)
                {
                    File.SetLastWriteTime(pathFile, lastWrite);
                }

            }
            catch (Exception)
            {
                return pathFile + tmpExt;
            }
        }
        return pathFile;
    }
    private void BakReadEnd(string pathFile)
    {
        if (File.Exists(pathFile + tmpExt) && (!File.Exists(pathFile) || File.GetCreationTime(pathFile + tmpExt) > File.GetLastWriteTime(pathFile)))
        {
            try
            {
                DateTime creation = File.GetCreationTime(pathFile + tmpExt);
                DateTime lastWrite = File.GetLastWriteTime(pathFile + tmpExt);

                File.Move(pathFile + tmpExt, pathFile, true);

#if DEBUG
                var trash10 = File.GetCreationTime(pathFile);
                var trash12 = File.GetLastWriteTime(pathFile);
#endif

                if (File.GetCreationTime(pathFile) != creation)
                {
                    File.SetCreationTime(pathFile, creation);
                }
                if (File.GetLastWriteTime(pathFile) != lastWrite)
                {
                    File.SetLastWriteTime(pathFile, lastWrite);
                }

            }
            catch (Exception)
            {
                return;
            }
        }
        if (File.Exists(pathFile + tmpExt) && File.Exists(pathFile) && File.GetCreationTime(pathFile + tmpExt) < File.GetLastWriteTime(pathFile))
        {
            try
            {
                File.Delete(pathFile + tmpExt);
            }
            catch (Exception)
            {

            }
        }
    }
}
