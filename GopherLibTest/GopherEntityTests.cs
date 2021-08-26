using GopherLib;
using GopherLib.Models;
using NUnit.Framework;

namespace GopherLibTest
{
    public class Tests
    {
        private const string DocLine = "0Does this gopher menu look correct?\t/gopher/proxy\tgopher.floodgap.com\t70";
        private const string InfoLine = "i(plus using the Floodgap Public Gopher Proxy)\t\terror.host\t1";
        private const string DirLine = "1Super-Dimensional Fortress: SDF Gopherspace\t\tsdf.org\t70";
        private const string SearchLine = "7Search Veronica-2\t/v2/vs\tgopher.floodgap.com\t70";
        private const string ErrorLine = "3Test, test, test\t\terror.host\t1\tExtra field\tAnother";

        private const string GopherFilePath = @"C:\Documents\file.gopher";
        private const string GopherFilePath2 = @"file:\\\C:\Documents\file.gopher";
        private const string OtherFilePath = @"C:\Documents\file.txt";

        private const string GopherDirUrl = @"gopher://gopher.floodgap.com/1/foo";
        private const string GopherSearchUrl = @"gopher://gopher.floodgap.com/7/v2/vs#search term";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GopherDirUrlTest()
        {
            var gopherEntity = new GopherEntity(GopherDirUrl, null);
            Assert.AreEqual(GopherEntity.DirectoryTypeChar, gopherEntity.Type);
            Assert.That(gopherEntity.DisplayText, Is.Null.Or.Empty);
            Assert.AreEqual("/foo", gopherEntity.Selector);
            Assert.AreEqual("gopher", gopherEntity.Scheme);
            Assert.AreEqual("gopher.floodgap.com", gopherEntity.Host);
            Assert.AreEqual(70, gopherEntity.Port);
            Assert.That(gopherEntity.Ignore, Is.Null.Or.Empty);
            Assert.That(gopherEntity.SearchTerms, Is.Null.Or.Empty);
        }

        [Test]
        public void GopherSearchUrlTest()
        {
            var gopherEntity = new GopherEntity(GopherSearchUrl, "Display Text");
            Assert.AreEqual(GopherEntity.IndexSearchTypeChar, gopherEntity.Type);
            Assert.AreEqual("Display Text", gopherEntity.DisplayText);
            Assert.AreEqual("/v2/vs", gopherEntity.Selector);
            Assert.AreEqual("gopher", gopherEntity.Scheme);
            Assert.AreEqual("gopher.floodgap.com", gopherEntity.Host);
            Assert.AreEqual(70, gopherEntity.Port);
            Assert.That(gopherEntity.Ignore, Is.Null.Or.Empty);
            Assert.AreEqual("search term", gopherEntity.SearchTerms);
        }

        [Test]
        public void GopherFilePathTest()
        {
            var gopherEntity = new GopherEntity(GopherFilePath, null);
            Assert.AreEqual(GopherEntity.DirectoryTypeChar, gopherEntity.Type);
            Assert.AreEqual(GopherFilePath, gopherEntity.DisplayText);
            Assert.That(gopherEntity.Selector, Is.Null.Or.Empty);
            Assert.AreEqual("file", gopherEntity.Scheme);
            Assert.That(gopherEntity.Host, Is.Null.Or.Empty);
            Assert.AreEqual(0, gopherEntity.Port);
            Assert.That(gopherEntity.Ignore, Is.Null.Or.Empty);
            Assert.That(gopherEntity.SearchTerms, Is.Null.Or.Empty);
        }

        [Test]
        public void GopherFilePath2Test()
        {
            var gopherEntity = new GopherEntity(GopherFilePath2, null);
            Assert.AreEqual(GopherEntity.DirectoryTypeChar, gopherEntity.Type);
            Assert.AreEqual(GopherFilePath, gopherEntity.DisplayText);
            Assert.That(gopherEntity.Selector, Is.Null.Or.Empty);
            Assert.AreEqual("file", gopherEntity.Scheme);
            Assert.That(gopherEntity.Host, Is.Null.Or.Empty);
            Assert.AreEqual(0, gopherEntity.Port);
            Assert.That(gopherEntity.Ignore, Is.Null.Or.Empty);
            Assert.That(gopherEntity.SearchTerms, Is.Null.Or.Empty);
        }

        [Test]
        public void OtherFilePathTest()
        {
            var gopherEntity = new GopherEntity(OtherFilePath, null);
            Assert.AreEqual(GopherEntity.DocumentTypeChar, gopherEntity.Type);
            Assert.AreEqual(OtherFilePath, gopherEntity.DisplayText);
            Assert.That(gopherEntity.Selector, Is.Null.Or.Empty);
            Assert.AreEqual("file", gopherEntity.Scheme);
            Assert.That(gopherEntity.Host, Is.Null.Or.Empty);
            Assert.AreEqual(0, gopherEntity.Port);
            Assert.That(gopherEntity.Ignore, Is.Null.Or.Empty);
            Assert.That(gopherEntity.SearchTerms, Is.Null.Or.Empty);
        }

        [Test]
        public void DocLineToEntityTest()
        {
            var gopherEntity = new GopherEntity(DocLine);
            Assert.AreEqual(GopherEntity.DocumentTypeChar, gopherEntity.Type);
            Assert.AreEqual("Does this gopher menu look correct?", gopherEntity.DisplayText);
            Assert.AreEqual("/gopher/proxy", gopherEntity.Selector);
            Assert.That(gopherEntity.Scheme, Is.Null.Or.Empty);
            Assert.AreEqual("gopher.floodgap.com", gopherEntity.Host);
            Assert.AreEqual(70, gopherEntity.Port);
            Assert.That(gopherEntity.Ignore, Is.Null.Or.Empty);
            Assert.That(gopherEntity.SearchTerms, Is.Null.Or.Empty);
        }

        [Test]
        public void InfoLineToEntityTest()
        {
            var gopherEntity = new GopherEntity(InfoLine);
            Assert.AreEqual(GopherEntity.InfoTypeChar, gopherEntity.Type);
            Assert.AreEqual("(plus using the Floodgap Public Gopher Proxy)", gopherEntity.DisplayText);
            Assert.That(gopherEntity.Selector, Is.Null.Or.Empty);
            Assert.That(gopherEntity.Scheme, Is.Null.Or.Empty);
            Assert.AreEqual("error.host", gopherEntity.Host);
            Assert.AreEqual(1, gopherEntity.Port);
            Assert.That(gopherEntity.Ignore, Is.Null.Or.Empty);
            Assert.That(gopherEntity.SearchTerms, Is.Null.Or.Empty);
        }

        [Test]
        public void DirLineToEntityTest()
        {
            var gopherEntity = new GopherEntity(DirLine);
            Assert.AreEqual(GopherEntity.DirectoryTypeChar, gopherEntity.Type);
            Assert.AreEqual("Super-Dimensional Fortress: SDF Gopherspace", gopherEntity.DisplayText);
            Assert.That(gopherEntity.Selector, Is.Null.Or.Empty);
            Assert.That(gopherEntity.Scheme, Is.Null.Or.Empty);
            Assert.AreEqual("sdf.org", gopherEntity.Host);
            Assert.AreEqual(70, gopherEntity.Port);
            Assert.That(gopherEntity.Ignore, Is.Null.Or.Empty);
            Assert.That(gopherEntity.SearchTerms, Is.Null.Or.Empty);
        }

        [Test]
        public void SearchLineToEntityTest()
        {
            var gopherEntity = new GopherEntity(SearchLine);
            Assert.AreEqual(GopherEntity.IndexSearchTypeChar, gopherEntity.Type);
            Assert.AreEqual("Search Veronica-2", gopherEntity.DisplayText);
            Assert.AreEqual("/v2/vs", gopherEntity.Selector);
            Assert.That(gopherEntity.Scheme, Is.Null.Or.Empty);
            Assert.AreEqual("gopher.floodgap.com", gopherEntity.Host);
            Assert.AreEqual(70, gopherEntity.Port);
            Assert.That(gopherEntity.Ignore, Is.Null.Or.Empty);
            Assert.That(gopherEntity.SearchTerms, Is.Null.Or.Empty);
        }

        [Test]
        public void ErrorLineToEntityTest()
        {
            var gopherEntity = new GopherEntity(ErrorLine);
            Assert.AreEqual(GopherEntity.ErrorTypeChar, gopherEntity.Type);
            Assert.AreEqual("Test, test, test", gopherEntity.DisplayText);
            Assert.That(gopherEntity.Selector, Is.Null.Or.Empty);
            Assert.That(gopherEntity.Scheme, Is.Null.Or.Empty);
            Assert.AreEqual("error.host", gopherEntity.Host);
            Assert.AreEqual(1, gopherEntity.Port);
            Assert.AreEqual("Extra field\tAnother", gopherEntity.Ignore);
            Assert.That(gopherEntity.SearchTerms, Is.Null.Or.Empty);
        }

        [Test]
        public void DocLineRoundTripTest()
        {
            Assert.AreEqual(DocLine, new GopherEntity(DocLine).ToString());
        }

        [Test]
        public void InfoLineRoundTripTest()
        {
            Assert.AreEqual(InfoLine, new GopherEntity(InfoLine).ToString());
        }

        [Test]
        public void DirLineRoundTripTest()
        {
            Assert.AreEqual(DirLine, new GopherEntity(DirLine).ToString());
        }

        [Test]
        public void SearchLineRoundTripTest()
        {
            Assert.AreEqual(SearchLine, new GopherEntity(SearchLine).ToString());
        }

        [Test]
        public void ErrorLineRoundTripTest()
        {
            Assert.AreEqual(ErrorLine, new GopherEntity(ErrorLine).ToString());
        }
    }
}