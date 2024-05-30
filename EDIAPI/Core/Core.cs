using EdiEngine;
using EdiEngine.Runtime;
using EdiEngine.Xml;
using System;
using System.Data.Common;
using System.IO;
using Azure.Storage.Blobs;
namespace EDIAPI.Services
{
    public static class Core
    {
     
        public static async Task  ProcessEdiFile(Stream ediStream, string blobName, BlobContainerClient  containerClient)
        {
            try
            {
                // Phân tích chuỗi EDI
                EdiDataReader reader = new EdiDataReader();
                EdiBatch batch = reader.FromStream(ediStream);

                // Chuyển đổi EDI sang XML
                XmlDataWriter xmlWriter = new XmlDataWriter();
                string xmlContent = xmlWriter.WriteToString(batch);

                // Lưu nội dung XML vào Blob Storage
                var blobClient = containerClient.GetBlobClient(blobName.Replace(".edi", ".xml"));
                using (var stream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(xmlContent);
                        writer.Flush();
                        stream.Position = 0;
                        blobClient.Upload(stream, true);
                    }
                }

                Console.WriteLine("File EDI đã được xử lý và lưu thành XML vào Blob Storage thành công.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi trong quá trình xử lý file EDI: {ex.Message}");
            }
        }
    }
}
