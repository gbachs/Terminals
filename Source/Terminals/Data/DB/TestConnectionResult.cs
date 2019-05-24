namespace Terminals.Data.DB
{
    /// <summary>
    ///     Container, which explains results of try to connect operations.
    /// </summary>
    internal class TestConnectionResult
    {
        /// <summary>
        ///     Initializes new successful connection result
        /// </summary>
        internal TestConnectionResult()
        {
            this.Successful = true;
        }

        /// <summary>
        ///     Initializes new not successful connection result.
        /// </summary>
        internal TestConnectionResult(string errorMessage)
        {
            this.Successful = false;
            this.ErroMessage = errorMessage;
        }

        protected TestConnectionResult(TestConnectionResult connectionResult)
        {
            this.Successful = connectionResult.Successful;
            this.ErroMessage = connectionResult.ErroMessage;
        }

        /// <summary>
        ///     Gets true, if try was successful and master password was successfully validated; otherwise false.
        /// </summary>
        internal bool Successful { get; }

        /// <summary>
        ///     Gets not empty string, if connection wasn't successful to explain the purpose.
        /// </summary>
        internal string ErroMessage { get; }

        public override string ToString()
        {
            return string.Format("TestConnectionResult:Successful={0},ErrorMessage={1}", this.Successful,
                this.ErroMessage);
        }
    }
}