using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace TestMakerFreeWebApp.ViewModels
{
    [JsonObject(MemberSerialization.OptOut)]
    public class AnswerViewModel
    {
        #region Constructor

        public AnswerViewModel()
        {
        }

        #endregion Constructor

        #region Properties

        public int Id { get; set; }

        public int QuizId { get; set; }

        public int QuestionId { get; set; }

        public string Text { get; set; }

        public string Notes { get; set; }

        [DefaultValue(0)]
        public int Type { get; set; }

        [DefaultValue(0)]
        public int Flags { get; set; }

        [DefaultValue(0)]
        public int Value { get; set; }

        [JsonIgnore]
        public DateTime CreatedDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        #endregion Properties
    }
}