namespace QuizNet
{
    public static class NetEventCodes
    {
        public const byte StartGame    = 1;
        public const byte SubmitAnswer = 2;
        public const byte NextQuestion = 3;
        public const byte GameOver     = 4;
        public const byte SyncTimer    = 5;
        public const byte PlayerInfo   = 6; // added
        public const byte Reveal       = 7; // show correct/wrong locally
    }
}
