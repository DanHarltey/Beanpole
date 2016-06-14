namespace Beanpole.Client
{
    using System;

    /// <summary>
    /// base class to save my fingers from typing disposable paten over and over
    /// </summary>
    public abstract class Disposable : IDisposable
    {
        private bool isDisposed;

        ~Disposable()
        {
            this.Dispose(false);
        }

        protected bool IsDisposed
        {
            get
            {
                return this.isDisposed;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(Disposable));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.isDisposed = true;
            }
        }
    }
}
