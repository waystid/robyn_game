using UnityEngine;
using System.Collections.Generic;

namespace CozyGame.Content
{
    /// <summary>
    /// Additional riddle content for the game.
    /// Contains 15 riddles of varying difficulty and themes.
    /// </summary>
    public static class AdditionalRiddles
    {
        /// <summary>
        /// Get all additional riddles
        /// </summary>
        public static List<RiddleDefinition> GetAllRiddles()
        {
            return new List<RiddleDefinition>
            {
                // Easy Riddles
                new RiddleDefinition
                {
                    riddleID = "riddle_seasons",
                    riddleName = "The Four Seasons",
                    difficulty = RiddleDifficulty.Easy,
                    riddleText = "I have no voice, yet I speak to all. I have no substance, yet I can be felt. I bring color to the world, yet I am invisible. What am I?",
                    correctAnswer = "wind",
                    alternateAnswers = new List<string> { "the wind", "air" },
                    hint = "It rustles the leaves and carries the clouds.",
                    experienceReward = 50,
                    category = RiddleCategory.Nature
                },

                new RiddleDefinition
                {
                    riddleID = "riddle_time",
                    riddleName = "Time's Arrow",
                    difficulty = RiddleDifficulty.Easy,
                    riddleText = "I have a face but no eyes, hands but no arms. I run but never walk. I tell you when but never where. What am I?",
                    correctAnswer = "clock",
                    alternateAnswers = new List<string> { "a clock", "watch", "a watch" },
                    hint = "You check me to know when to meet your friends.",
                    experienceReward = 50,
                    category = RiddleCategory.Object
                },

                new RiddleDefinition
                {
                    riddleID = "riddle_mirror",
                    riddleName = "The Silent Twin",
                    difficulty = RiddleDifficulty.Easy,
                    riddleText = "I show you yourself, yet I am not you. I copy your every move, yet I make no sound. I exist in stillness, yet I capture motion. What am I?",
                    correctAnswer = "mirror",
                    alternateAnswers = new List<string> { "a mirror", "reflection" },
                    hint = "You look at me every morning to check your appearance.",
                    experienceReward = 50,
                    category = RiddleCategory.Object
                },

                // Medium Riddles
                new RiddleDefinition
                {
                    riddleID = "riddle_river",
                    riddleName = "The Endless Journey",
                    difficulty = RiddleDifficulty.Medium,
                    riddleText = "I have a bed but never sleep. I have a mouth but never speak. I run but have no feet. I have banks but no money. What am I?",
                    correctAnswer = "river",
                    alternateAnswers = new List<string> { "a river", "stream", "a stream" },
                    hint = "Fish live in me, and I flow to the sea.",
                    experienceReward = 100,
                    itemReward = "wisdom_token",
                    category = RiddleCategory.Nature
                },

                new RiddleDefinition
                {
                    riddleID = "riddle_book",
                    riddleName = "The Silent Teacher",
                    difficulty = RiddleDifficulty.Medium,
                    riddleText = "I have a spine but no bones. I have a cover but I'm not a bed. I have pages but I'm not a calendar. I hold stories but I cannot speak. What am I?",
                    correctAnswer = "book",
                    alternateAnswers = new List<string> { "a book", "tome" },
                    hint = "You read me to gain knowledge and entertainment.",
                    experienceReward = 100,
                    category = RiddleCategory.Knowledge
                },

                new RiddleDefinition
                {
                    riddleID = "riddle_candle",
                    riddleName = "The Selfless Light",
                    difficulty = RiddleDifficulty.Medium,
                    riddleText = "I give light but am not the sun. I grow shorter the longer I live. I cry but have no eyes. I die to serve you. What am I?",
                    correctAnswer = "candle",
                    alternateAnswers = new List<string> { "a candle" },
                    hint = "I'm made of wax and have a wick.",
                    experienceReward = 100,
                    itemReward = "wisdom_token",
                    category = RiddleCategory.Object
                },

                new RiddleDefinition
                {
                    riddleID = "riddle_echo",
                    riddleName = "The Returning Voice",
                    difficulty = RiddleDifficulty.Medium,
                    riddleText = "I speak without a mouth and hear without ears. I have no body, but come alive with wind. I am everywhere, yet nowhere. What am I?",
                    correctAnswer = "echo",
                    alternateAnswers = new List<string> { "an echo", "sound" },
                    hint = "Shout in a canyon and you'll hear me.",
                    experienceReward = 100,
                    category = RiddleCategory.Abstract
                },

                // Hard Riddles
                new RiddleDefinition
                {
                    riddleID = "riddle_nothing",
                    riddleName = "The Paradox of Absence",
                    difficulty = RiddleDifficulty.Hard,
                    riddleText = "I am weightless, but you can see me. Put me in a barrel and I make it lighter. I am greater than the gods, more evil than the devil. The poor have me, the rich need me, and if you eat me you will die. What am I?",
                    correctAnswer = "nothing",
                    alternateAnswers = new List<string> { "none", "emptiness", "void" },
                    hint = "I am the absence of everything.",
                    experienceReward = 200,
                    itemReward = "ancient_wisdom_scroll",
                    unlockLoreEntry = "philosophy_of_void",
                    category = RiddleCategory.Abstract
                },

                new RiddleDefinition
                {
                    riddleID = "riddle_tomorrow",
                    riddleName = "The Eternal Chase",
                    difficulty = RiddleDifficulty.Hard,
                    riddleText = "I am always coming but never arrive. I am always ahead but never here. Chase me and you'll never catch me. Wait for me and I become today. What am I?",
                    correctAnswer = "tomorrow",
                    alternateAnswers = new List<string> { "the future", "future" },
                    hint = "Time-related, always one day away.",
                    experienceReward = 200,
                    itemReward = "ancient_wisdom_scroll",
                    category = RiddleCategory.Time
                },

                new RiddleDefinition
                {
                    riddleID = "riddle_shadow",
                    riddleName = "The Dark Companion",
                    difficulty = RiddleDifficulty.Hard,
                    riddleText = "I follow you by day but not by night. I grow when light is bright, shrink when it dims. I can be stepped on but never hurt. I mimic your form but have no substance. What am I?",
                    correctAnswer = "shadow",
                    alternateAnswers = new List<string> { "a shadow", "shadows" },
                    hint = "Light creates me, darkness hides me.",
                    experienceReward = 200,
                    category = RiddleCategory.Nature
                },

                // Expert Riddles
                new RiddleDefinition
                {
                    riddleID = "riddle_silence",
                    riddleName = "The Loudest Quiet",
                    difficulty = RiddleDifficulty.Expert,
                    riddleText = "I can be broken without being held. I can be kept without being touched. I can be betrayed without being seen. I can be precious yet cost nothing. What am I?",
                    correctAnswer = "silence",
                    alternateAnswers = new List<string> { "quiet", "peace" },
                    hint = "Libraries treasure me, concerts break me.",
                    experienceReward = 300,
                    goldReward = 200,
                    itemReward = "philosopher_stone_fragment",
                    unlockLoreEntry = "ancient_philosophers",
                    category = RiddleCategory.Abstract
                },

                new RiddleDefinition
                {
                    riddleID = "riddle_breath",
                    riddleName = "The Invisible Lifeline",
                    difficulty = RiddleDifficulty.Expert,
                    riddleText = "I am taken before I'm given. I am invisible yet essential. I am first when you enter the world and last when you leave. I can be held but not forever. What am I?",
                    correctAnswer = "breath",
                    alternateAnswers = new List<string> { "a breath", "breathing", "air" },
                    hint = "You do this about 20,000 times a day.",
                    experienceReward = 300,
                    goldReward = 200,
                    unlockSpellID = "wind_mastery",
                    category = RiddleCategory.Nature
                },

                new RiddleDefinition
                {
                    riddleID = "riddle_fire",
                    riddleName = "The Dancing Destroyer",
                    difficulty = RiddleDifficulty.Expert,
                    riddleText = "I am born from nothing, yet I consume all. I dance without feet, sing without voice. I bring warmth yet can destroy. I am alive yet not living. Feed me and I grow, give me water and I die. What am I?",
                    correctAnswer = "fire",
                    alternateAnswers = new List<string> { "flame", "flames" },
                    hint = "I need oxygen, fuel, and heat to exist.",
                    experienceReward = 300,
                    goldReward = 200,
                    unlockSpellID = "flame_mastery",
                    category = RiddleCategory.Element
                },

                // Logic Riddles
                new RiddleDefinition
                {
                    riddleID = "riddle_hole",
                    riddleName = "The Growing Absence",
                    difficulty = RiddleDifficulty.Medium,
                    riddleText = "The more you take away, the larger I become. The more you add, the smaller I get. I can be deep or shallow, round or square. What am I?",
                    correctAnswer = "hole",
                    alternateAnswers = new List<string> { "a hole", "pit" },
                    hint = "Digging makes me bigger.",
                    experienceReward = 100,
                    category = RiddleCategory.Logic
                },

                new RiddleDefinition
                {
                    riddleID = "riddle_secret",
                    riddleName = "The Shared Burden",
                    difficulty = RiddleDifficulty.Hard,
                    riddleText = "I am worthless when shared. I can destroy trust when revealed. I can be kept for years or spilled in seconds. The more people who know me, the less value I have. What am I?",
                    correctAnswer = "secret",
                    alternateAnswers = new List<string> { "a secret", "secrets" },
                    hint = "Two can keep me if one of them is dead.",
                    experienceReward = 200,
                    itemReward = "ancient_wisdom_scroll",
                    category = RiddleCategory.Abstract
                }
            };
        }

        /// <summary>
        /// Riddle difficulty
        /// </summary>
        public enum RiddleDifficulty
        {
            Easy,
            Medium,
            Hard,
            Expert
        }

        /// <summary>
        /// Riddle category
        /// </summary>
        public enum RiddleCategory
        {
            Nature,
            Object,
            Knowledge,
            Abstract,
            Time,
            Element,
            Logic
        }

        /// <summary>
        /// Riddle definition
        /// </summary>
        [System.Serializable]
        public class RiddleDefinition
        {
            public string riddleID;
            public string riddleName;
            public RiddleDifficulty difficulty;
            public RiddleCategory category;
            public string riddleText;
            public string correctAnswer;
            public List<string> alternateAnswers = new List<string>();
            public string hint;

            // Rewards
            public int experienceReward;
            public int goldReward;
            public string itemReward;

            // Unlocks
            public string unlockSpellID;
            public string unlockLoreEntry;
        }
    }
}
