using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.WindowsAzure.Storage.Blob;
using Serilog.Events;
using Serilog.Formatting;
using Xunit;

namespace Serilog.Sinks.AzureBlobStorage.UnitTest
{
    public class DefaultAppendBlobBlockWriterUT
    {
        readonly DefaultAppendBlobBlockWriter _defaultAppendBlobBlockWriter;

        readonly CloudAppendBlob _cloudBlobFake= A.Fake<CloudAppendBlob>(opt=> opt.WithArgumentsForConstructor(new[] { new Uri("https://blob.com/test/test.txt") }));

        readonly IEnumerable<string> _noBlocksToWrite = Enumerable.Empty<string>();
        readonly IEnumerable<string> _singleBlockToWrite = new[] { new string('*', 1024 * 1024 * 3) };
        readonly IEnumerable<string> _multipleBlocksToWrite = new[] { new string('*', 1024 * 512 * 3), new string('*', 1024 * 512 * 3) };

        readonly ICollection<Stream> _writtenBlocks = new List<Stream>();

        public DefaultAppendBlobBlockWriterUT()
        {
            _defaultAppendBlobBlockWriter = new DefaultAppendBlobBlockWriter();
        }

        [Fact(DisplayName = "Should not write anything when no blocks to write.")]
        public async Task WriteNothingIfNoBlocksSent()
        {
            await _defaultAppendBlobBlockWriter.WriteBlocksToAppendBlobAsync(_cloudBlobFake, _noBlocksToWrite);

            A.CallTo(() => _cloudBlobFake.AppendBlockAsync(A<Stream>.Ignored, null)).MustNotHaveHappened();
        }

        [Fact(DisplayName = "Should write as many blocks as going in, one.")]
        public async Task WriteSingleBlockOnSingleInput()
        {
            await _defaultAppendBlobBlockWriter.WriteBlocksToAppendBlobAsync(_cloudBlobFake, _singleBlockToWrite);

            A.CallTo(() => _cloudBlobFake.AppendBlockAsync(A<Stream>.Ignored, null)).MustHaveHappenedOnceExactly();
        }

        [Fact(DisplayName = "Should write as many blocks as going in, two.")]
        public async Task WriteTwoBlocksOnOnInputOfTwo()
        {
            await _defaultAppendBlobBlockWriter.WriteBlocksToAppendBlobAsync(_cloudBlobFake, _multipleBlocksToWrite);

            A.CallTo(() => _cloudBlobFake.AppendBlockAsync(A<Stream>.Ignored, null)).MustHaveHappened(_multipleBlocksToWrite.Count(), Times.Exactly);
        }
    }
}