using BaseLibrary;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace BaseLibraryFile.MSTest;

[TestClass]
public class FileServicesTextTest
{
    [TestMethod]
    public void WriteTXTDeveRetornaArgumentNullExceptionSePathFileENull()
    {
        var serviceText = new FileServicesText();
        Exception nullTeste = null;
        try
        {
            serviceText.WriteTXT(null, null);
        }
        catch (Exception e)
        {
            nullTeste = e;
        }
        Assert.IsNotNull(nullTeste);
        Assert.IsInstanceOfType(nullTeste, typeof(ArgumentNullException));
        Assert.AreEqual(((ArgumentNullException)nullTeste).ParamName, "pathFile");
    }
    [TestMethod]
    public void WriteTXTDeveRetornaArgumentNullExceptionSeParTextENull()
    {
        var serviceText = new FileServicesText();
        Exception nullTeste = null;
        try
        {
            serviceText.WriteTXT(folderTest, null);
        }
        catch (Exception e)
        {
            nullTeste = e;
        }
        Assert.IsNotNull(nullTeste);
        Assert.IsInstanceOfType(nullTeste, typeof(ArgumentNullException));
        if (nullTeste is ArgumentNullException)
            Assert.AreEqual(((ArgumentNullException)nullTeste).ParamName, "parTXT");
    }
    [TestMethod]
    public void WriteTXTDeveEscreverOArquivoText()
    {
        PreparerDirectoryTest();
        var serviceText = new FileServicesText();
        Exception nullTeste = null;
        bool? result = null;

        string textToFile = "test";
        try
        {
            result = serviceText.WriteTXT(fileText, textToFile);
        }
        catch (Exception e)
        {
            nullTeste = e;
        }
        Assert.IsNull(nullTeste);
        Assert.IsTrue(result);
        //TODO tenho que verificar se tem o arquivo mesmo e verificar seu conteúdo
        Assert.IsTrue(File.Exists(fileText));
        Assert.IsFalse(File.Exists(fileText + ".tmp"));
        Assert.AreEqual(File.ReadAllText(fileText), textToFile);
    }
    public void WriteTXTDeveRetornarFalseQuandoOArquivoEAssessadoPorOutroProcesso()
    {
        PreparerDirectoryTest();
        var serviceText = new FileServicesText();
        Exception nullTeste = null;
        bool? result = null;

        string textToFile = "test";
        try
        {
            result = serviceText.WriteTXT(fileText, textToFile);
        }
        catch (Exception e)
        {
            nullTeste = e;
        }
        Assert.IsNull(nullTeste);
        Assert.IsTrue(result);
        //TODO tenho que verificar se tem o arquivo mesmo e verificar seu conteúdo
        Assert.IsTrue(File.Exists(fileText));
        Assert.IsFalse(File.Exists(fileText + ".tmp"));
        Assert.AreEqual(File.ReadAllText(fileText), textToFile);
    }

    string folderTest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Nimloth Testes");
    string fileText = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Nimloth Testes", "TestFile.txt");
    private void PreparerDirectoryTest()
    {

        if (Directory.Exists(folderTest))
        {
            Directory.Delete(folderTest, true);
        }
        Directory.CreateDirectory(folderTest);
    }
}