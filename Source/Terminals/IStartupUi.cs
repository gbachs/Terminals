using Terminals.Data;

namespace Terminals
{
    internal interface IStartupUi
    {
        bool UserWantsFallback();

        AuthenticationPrompt KnowsUserPassword(bool previousTrySuccess);

        void Exit();
    }
}