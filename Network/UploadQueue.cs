﻿using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace aevvuploader.Network
{
    public class UploadQueue
    {
        private readonly Queue<Bitmap> _queue;
        private readonly object _sync;
        private readonly ImageUploader _uploader;
        private readonly UploadResultHandler _handler;

        public UploadQueue(ImageUploader uploader, UploadResultHandler handler)
        {
            _sync = new object();
            _queue = new Queue<Bitmap>();
            _uploader = uploader;
            _handler = handler;

            Task.Factory.StartNew(Upload);
        }

        public void QueueImage(Bitmap bitmap)
        {
            lock (_sync)
            {
                _queue.Enqueue(bitmap);
            }
        }

        private void Upload()
        {
            // TODO: Cancellation
            while (true)
            {
                Bitmap bitmap;
                lock (_sync)
                {
                    if (_queue.Count == 0)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    bitmap = _queue.Dequeue();
                }

                var result = _uploader.UploadSync(bitmap);

                _handler.HandleResult(result);

                Thread.Sleep(100);
            }
        }
    }
}