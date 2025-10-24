#if PLAYFAB_INSTALLED
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using QuizGame.Models;
using QuizGame.UI;
using UnityEngine;
using Newtonsoft.Json;

namespace QuizGame.Services
{
    public class PlayFabAuthService : MonoBehaviour, IAuthService
    {
        public string PlayerId { get; private set; }
        public string DisplayName { get; private set; }

        string StatisticNameTable;

        #region Login/Signup/Guest

        // ---------------------- EMAIL LOGIN ----------------------
        public async Task<bool> LoginAsync(string email, string password)
        {
            var req = new LoginWithEmailAddressRequest
            {
                Email = email,
                Password = password,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true
                }
            };
            var tcs = new TaskCompletionSource<bool>();
            PlayFabClientAPI.LoginWithEmailAddress(req, async result =>
            {
                PlayerId = result.PlayFabId;
                DisplayName = result.InfoResultPayload?.PlayerProfile?.DisplayName ?? "Player";
                UnityEngine.Debug.Log($"player id: {PlayerId}, name: {DisplayName}");

                BootstrapGame.instance.IsGuest = false;

                // Ensure topics + stats are synced on every login for registered users
                await SyncPlayerDataWithTopics();

                tcs.SetResult(true);
            }, error =>
            {
                UnityEngine.Debug.LogError($"PlayFab login failed: {error.GenerateErrorReport()}");
                tcs.SetResult(false);
            });
            return await tcs.Task;
        }

        // ---------------------- GUEST LOGIN ----------------------
        public async Task<bool> LoginWithCustomIdAsync(string username = null)
        {
            var req = new LoginWithCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
                CreateAccount = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true
                }
            };
            var tcs = new TaskCompletionSource<bool>();

            PlayFabClientAPI.LoginWithCustomID(req, async result =>
            {
                PlayerId = result.PlayFabId;
                DisplayName = result.InfoResultPayload?.PlayerProfile?.DisplayName;
                BootstrapGame.instance.IsGuest = true;

                if (!string.IsNullOrEmpty(username))
                {
                    bool usernameSet = await SetDisplayNameAsync(username);
                    if (!usernameSet)
                    {
                        UnityEngine.Debug.LogWarning("Username already taken. Please choose another.");
                        tcs.SetResult(false);
                        return;
                    }
                }

                if (string.IsNullOrEmpty(DisplayName))
                {
                    string uniqueName = GenerateFunUniqueName();
                    bool nameSet = await SetDisplayNameAsync(uniqueName);
                    if (!nameSet)
                    {
                        uniqueName += "_" + UnityEngine.Random.Range(1000, 9999);
                        await SetDisplayNameAsync(uniqueName);
                    }
                }

                GetAllData();
                tcs.SetResult(true);
            },
            error =>
            {
                UnityEngine.Debug.LogError($"PlayFab custom id login failed: {error.GenerateErrorReport()}");
                tcs.SetResult(false);
            });

            return await tcs.Task;
        }

        private async Task<bool> SetDisplayNameAsync(string name)
        {
            var tcs = new TaskCompletionSource<bool>();
            var req = new UpdateUserTitleDisplayNameRequest { DisplayName = name };
            PlayFabClientAPI.UpdateUserTitleDisplayName(req, result =>
            {
                DisplayName = result.DisplayName;
                tcs.SetResult(true);
            }, error =>
            {
                if (error.Error == PlayFabErrorCode.NameNotAvailable)
                    tcs.SetResult(false);
                else
                {
                    UnityEngine.Debug.LogError("Failed to set display name: " + error.GenerateErrorReport());
                    tcs.SetResult(false);
                }
            });
            return await tcs.Task;
        }

        private string GenerateFunUniqueName()
        {
            string[] adjectives = { "Crazy", "Happy", "Silly", "Brave", "Smart", "Lucky", "Funky" };
            string[] animals = { "Tiger", "Panda", "Elephant", "Monkey", "Fox", "Dragon", "Lion" };
            int number = UnityEngine.Random.Range(100, 999);
            return adjectives[UnityEngine.Random.Range(0, adjectives.Length)] +
                   animals[UnityEngine.Random.Range(0, animals.Length)] +
                   number;
        }

        // ---------------------- SIGNUP ----------------------
        public async Task<bool> SignupAsync(string email, string password, string username = null)
        {
            var req = new RegisterPlayFabUserRequest
            {
                Email = email,
                Password = password,
                Username = username,
                RequireBothUsernameAndEmail = false
            };

            var tcs = new TaskCompletionSource<bool>();

            PlayFabClientAPI.RegisterPlayFabUser(req, async result =>
            {
                PlayerId = result.PlayFabId;
                DisplayName = username ?? "Player";
                BootstrapGame.instance.IsGuest = false;

                if (!string.IsNullOrEmpty(username))
                {
                    var updateDisplayNameRequest = new UpdateUserTitleDisplayNameRequest
                    {
                        DisplayName = username
                    };

                    PlayFabClientAPI.UpdateUserTitleDisplayName(updateDisplayNameRequest, async updateResult =>
                    {
                        DisplayName = updateResult.DisplayName;
                        await InitializePlayerDataAndStats(); // init topics at signup
                        tcs.SetResult(true);
                    }, error =>
                    {
                        UnityEngine.Debug.LogWarning("Failed to set display name: " + error.GenerateErrorReport());
                        tcs.SetResult(false);
                    });
                }
                else
                {
                    await InitializePlayerDataAndStats();
                    tcs.SetResult(true);
                }
            },
            error =>
            {
                UnityEngine.Debug.LogError($"PlayFab signup failed: {error.GenerateErrorReport()}");
                tcs.SetResult(false);
            });

            return await tcs.Task;
        }

        #endregion

        #region Leaderboard

        public async Task<bool> LeaderBoard(int score, string topicId)
        {
            if (BootstrapGame.instance.IsGuest)
            {
                UnityEngine.Debug.Log("Guest user → not submitting leaderboard score.");
                return false;
            }

            var request = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate { StatisticName = topicId, Value = score }
                }
            };

            var tcs = new TaskCompletionSource<bool>();
            PlayFabClientAPI.UpdatePlayerStatistics(request, result =>
            {
                UnityEngine.Debug.Log($"Successfully updated leaderboard for {topicId}.");
                tcs.SetResult(true);
            }, error =>
            {
                UnityEngine.Debug.LogError($"PlayFab leaderboard update failed: {error.GenerateErrorReport()}");
                tcs.SetResult(false);
            });

            return await tcs.Task;
        }

        public async Task<bool> GetLeaderBoard(GameObject ListingPrefab, Transform ListingContainer)
        {
            string topicId = GetCurrentTopicKey();

            var request = new GetLeaderboardRequest
            {
                StatisticName = topicId,
                StartPosition = 0,
                MaxResultsCount = 10
            };

            var tcs = new TaskCompletionSource<bool>();
            PlayFabClientAPI.GetLeaderboard(request, result =>
            {
                foreach (Transform child in ListingContainer)
                    GameObject.Destroy(child.gameObject);

                foreach (PlayerLeaderboardEntry entry in result.Leaderboard)
                {
                    GameObject listingObj = GameObject.Instantiate(ListingPrefab, ListingContainer);
                    LeaderBoardListingUI listingUI = listingObj.GetComponent<LeaderBoardListingUI>();
                    if (listingUI != null)
                    {
                        listingUI.PlayerIndexText.text = (entry.Position + 1).ToString();
                        listingUI.PlayerNameText.text = entry.DisplayName ?? "Anonymous";
                        listingUI.PlayerScoreText.text = entry.StatValue.ToString();
                    }
                }

                UnityEngine.Debug.Log("Successfully retrieved leaderboard for " + topicId);
                tcs.SetResult(true);
            }, error =>
            {
                UnityEngine.Debug.LogError($"PlayFab leaderboard retrieval failed: {error.GenerateErrorReport()}");
                tcs.SetResult(false);
            });

            return await tcs.Task;
        }
        #endregion

        #region Data Initialization / Sync

        private async Task InitializePlayerDataAndStats()
        {
            // Fetch TitleData (QuizTopicsData)
            var getTitleDataTcs = new TaskCompletionSource<Dictionary<string, string>>();
            PlayFabClientAPI.GetTitleData(new GetTitleDataRequest
            {
                Keys = new List<string> { "QuizTopicsData" }
            },
            result => getTitleDataTcs.SetResult(result.Data),
            error =>
            {
                UnityEngine.Debug.LogError("Failed to fetch topics: " + error.GenerateErrorReport());
                getTitleDataTcs.SetResult(null);
            });

            var data = await getTitleDataTcs.Task;
            if (data == null || !data.ContainsKey("QuizTopicsData"))
                return;

            // TitleData JSON is the raw QuizDatabase: { "topics": [ ... ] }
            QuizDatabase quizDb = null;
            try
            {
                quizDb = JsonConvert.DeserializeObject<QuizDatabase>(data["QuizTopicsData"]);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse QuizTopicsData: " + ex);
                return;
            }

            if (quizDb?.topics == null)
            {
                Debug.LogWarning("No topics found in TitleData.");
                return;
            }

            var newDict = new Dictionary<string, int>();
            var statistics = new List<StatisticUpdate>();

            foreach (var topic in quizDb.topics)
            {
                newDict[topic.topicId] = 0;
                statistics.Add(new StatisticUpdate { StatisticName = topic.topicId, Value = 0 });
            }

            // Save WinCounts (using your Serialization helper so existing stored format remains compatible)
            string json = JsonUtility.ToJson(new Serialization<string, int>(newDict));
            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> { { "WinCounts", json } }
            }, result => { Debug.Log("Initialized WinCounts for new player."); }, error =>
            {
                Debug.LogError("Failed to save initial WinCounts: " + error.GenerateErrorReport());
            });

            // Push statistics so player appears on leaderboards with 0
            if (statistics.Count > 0)
            {
                PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
                {
                    Statistics = statistics
                }, result => { Debug.Log("Initialized leaderboard stats for new player."); }, error =>
                {
                    Debug.LogError("Failed to init leaderboard stats: " + error.GenerateErrorReport());
                });
            }

            BootstrapGame.instance.winCounts = newDict;
        }

        public void GetAllData()
        {
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), async result =>
            {
                Dictionary<string, int> localCounts;

                if (result.Data != null && result.Data.TryGetValue("WinCounts", out var record))
                {
                    try
                    {
                        var data = JsonUtility.FromJson<Serialization<string, int>>(record.Value);
                        localCounts = data.ToDictionary();
                    }
                    catch (Exception)
                    {
                        // fallback to empty
                        localCounts = new Dictionary<string, int>();
                    }
                }
                else
                {
                    localCounts = new Dictionary<string, int>();
                }

                // Sync with TitleData (add new topics, remove obsolete)
                await SyncPlayerDataWithTopics(localCounts);
            }, error =>
            {
                UnityEngine.Debug.LogError($"Failed to retrieve user data: {error.GenerateErrorReport()}");
            });
        }

        /// <summary>
        /// Ensures the user's WinCounts and PlayerStatistics include all current topics from TitleData.
        /// If existingCounts is null, creates a fresh dictionary.
        /// </summary>
        private async Task SyncPlayerDataWithTopics(Dictionary<string, int> existingCounts = null)
        {
            // Fetch TitleData (QuizTopicsData)
            var getTitleDataTcs = new TaskCompletionSource<Dictionary<string, string>>();
            PlayFabClientAPI.GetTitleData(new GetTitleDataRequest
            {
                Keys = new List<string> { "QuizTopicsData" }
            },
            result => getTitleDataTcs.SetResult(result.Data),
            error =>
            {
                UnityEngine.Debug.LogError("Failed to fetch TitleData: " + error.GenerateErrorReport());
                getTitleDataTcs.SetResult(null);
            });

            var tdData = await getTitleDataTcs.Task;
            if (tdData == null || !tdData.ContainsKey("QuizTopicsData"))
                return;

            QuizDatabase quizDb = null;
            try
            {
                quizDb = JsonConvert.DeserializeObject<QuizDatabase>(tdData["QuizTopicsData"]);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse QuizTopicsData: " + ex);
                return;
            }

            if (quizDb?.topics == null)
                return;

            if (existingCounts == null)
                existingCounts = new Dictionary<string, int>();

            bool updated = false;
            var stats = new List<StatisticUpdate>();

            // Add missing topics (set to 0) and build stats list
            foreach (var topic in quizDb.topics)
            {
                if (!existingCounts.ContainsKey(topic.topicId))
                {
                    existingCounts[topic.topicId] = 0;
                    updated = true;
                }
                stats.Add(new StatisticUpdate { StatisticName = topic.topicId, Value = existingCounts[topic.topicId] });
            }

            // Remove obsolete topics no longer in TitleData
            var obsolete = new List<string>();
            foreach (var key in existingCounts.Keys)
            {
                bool found = false;
                foreach (var t in quizDb.topics)
                {
                    if (t.topicId == key) { found = true; break; }
                }
                if (!found) obsolete.Add(key);
            }
            foreach (var key in obsolete)
            {
                existingCounts.Remove(key);
                updated = true;
            }

            // Save updated WinCounts if we changed anything
            if (updated)
            {
                string json = JsonUtility.ToJson(new Serialization<string, int>(existingCounts));
                PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
                {
                    Data = new Dictionary<string, string> { { "WinCounts", json } }
                }, result => { Debug.Log("Updated WinCounts after syncing topics."); }, error =>
                {
                    Debug.LogError("Failed to update WinCounts: " + error.GenerateErrorReport());
                });
            }

            // Push statistics so the player exists on leaderboards (even 0s)
            if (stats.Count > 0)
            {
                PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
                {
                    Statistics = stats
                }, result => { Debug.Log("Synced player statistics with TitleData topics."); }, error =>
                {
                    Debug.LogError("Failed to sync player statistics: " + error.GenerateErrorReport());
                });
            }

            BootstrapGame.instance.winCounts = existingCounts;
        }

        public void SaveAllData()
        {
            string json = JsonUtility.ToJson(new Serialization<string, int>(BootstrapGame.instance.winCounts));

            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> { { "WinCounts", json } }
            };

            PlayFabClientAPI.UpdateUserData(request, result =>
            {
                UnityEngine.Debug.Log("All win counts saved.");
            }, error =>
            {
                UnityEngine.Debug.LogError($"Failed to save data: {error.GenerateErrorReport()}");
            });
        }

        #endregion

        #region Topic Key (dynamic)

        public string GetCurrentTopicKey()
        {
            string topicId = TopicUI.instance.selectedTopic.topicId;
            StatisticNameTable = topicId;
            return topicId;
        }

        #endregion
    }

    // Helper class for serializing Dictionary<string, int>
    [System.Serializable]
    public class Serialization<TKey, TValue>
    {
        [System.Serializable]
        public struct KeyValue
        {
            public TKey Key;
            public TValue Value;
        }

        public List<KeyValue> items = new List<KeyValue>();

        public Serialization(Dictionary<TKey, TValue> dict)
        {
            foreach (var pair in dict)
                items.Add(new KeyValue { Key = pair.Key, Value = pair.Value });
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dict = new Dictionary<TKey, TValue>();
            foreach (var item in items)
                dict[item.Key] = item.Value;
            return dict;
        }
    }
}
#endif
