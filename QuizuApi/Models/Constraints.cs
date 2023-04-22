namespace QuizuApi.Models
{
    public static class Constraints
    {
        public const int LabelLengthMin = 3;
        public const int LabelLengthMax = 25;
        public const int AboutLengthMax = 1000;
        public const int AnswerLengthMin = 1;
        public const int AnswerLengthMax = 255;
        public const int QuestionLengthMin = 5;
        public const int QuestionLengthMax = 255;
        public const int AnswerListLengthMin = 1;
        public const int AnswerListLengthMax = 4;
        public const int TitleLengthMin = 5;
        public const int TitleLengthMax = 100;
        public const int DescriptionLengthMin = 0;
        public const int DescriptionLengthMax = 255;
        public const int TagsListLengthMin = 0;
        public const int TagsListLengthMax = 5;
        public const int CommentLengthMin = 1;
        public const int CommentLengthMax = 1000;
        public const int RefreshTokenLength = 255;
        public const int PasswordLengthMin = 8;
        public const int PasswordLengthMax = 255;
    }
}
