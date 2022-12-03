using System;
using System.Security.Cryptography;
using System.Text;
using Altzone.Scripts.Model;
using Prg.Scripts.Common.Unity.Localization;
using UnityEngine;

namespace Altzone.Scripts.Config
{
    public interface IPlayerDataCache
    {
        string PlayerName { get; set; }
        string PlayerGuid { get; }
        int ClanId { get; set; }
        int CustomCharacterModelId { get; set; }
        SystemLanguage Language { get; set; }
        bool IsDebugFlag { get; set; }
        bool IsTosAccepted { get; set; }
        bool IsFirstTimePlaying { get; set; }
        bool IsAccountVerified { get; set; }

        //IBattleCharacter CurrentBattleCharacter { get; }
        ClanModel Clan { get; }

        bool HasPlayerName { get; }
        void UpdatePlayerGuid(string newPlayerGuid);

#if UNITY_EDITOR
        void DebugSavePlayer();
        void DebugResetPlayer();
#endif
    }

    /// <summary>
    /// Player data cache - a common storage for player related data that is persisted somewhere (locally).
    /// </summary>
    [Serializable]
    public class PlayerDataCache : IPlayerDataCache
    {
        public static IPlayerDataCache Create()
        {
            return new PlayerDataCacheLocal();
        }

        public int CustomCharacterModelId
        {
            get => 1;
            set => throw new NotImplementedException();
        }

        public bool IsFirstTimePlaying
        {
            get => false;
            set => throw new NotImplementedException();
        }

        public bool IsAccountVerified
        {
            get => false;
            set => throw new NotImplementedException();
        }

        public ClanModel Clan => throw new NotImplementedException();

        public void UpdatePlayerGuid(string newPlayerGuid)
        {
            throw new NotImplementedException();
        }

        public void DebugSavePlayer()
        {
            throw new NotImplementedException();
        }

        [SerializeField] protected string _playerName;

        /// <summary>
        /// Player name.
        /// </summary>
        /// <remarks>
        /// This should be validated and sanitized before accepting a new value.
        /// </remarks>
        public string PlayerName
        {
            get => _playerName;
            set
            {
                _playerName = value ?? string.Empty;
                Save();
            }
        }

        public bool HasPlayerName => !string.IsNullOrWhiteSpace(_playerName);

        [SerializeField] protected int _characterModelId;

        /// <summary>
        /// Player character model id.
        /// </summary>
        public int CharacterModelId
        {
            get => _characterModelId;
            set
            {
                _characterModelId = value;
                Save();
            }
        }

        /// <summary>
        /// Player character model.
        /// </summary>
        /// <remarks>
        /// This is guaranteed to be valid reference all the time even <c>CharacterModelId</c> is invalid.
        /// </remarks>
        public CharacterModel CharacterModel =>
            Storefront.Get().GetCharacterModel(_characterModelId) ??
            new CharacterModel(-1, "Ö", Defence.Desensitisation, 0, 0, 0, 0);

        [SerializeField] protected int _clanId;

        /// <summary>
        /// Player clan id.
        /// </summary>
        public int ClanId
        {
            get => _clanId;
            set
            {
                _clanId = value;
                Save();
            }
        }

        [SerializeField] protected string _playerGuid;

        /// <summary>
        /// Unique string to identify this player across devices and systems.
        /// </summary>
        /// <remarks>
        /// When new player is detected this could be set and persisted in all external systems in order to identify this player unambiguously.
        /// </remarks>
        public string PlayerGuid
        {
            get => _playerGuid;
            set
            {
                _playerGuid = value ?? string.Empty;
                Save();
            }
        }

        /// <summary>
        /// Player's UNITY language.
        /// </summary>
        /// <remarks>
        /// We use <c>Localizer</c> to do actual storing of the language.
        /// </remarks>
        public SystemLanguage Language
        {
            get => Localizer.Language;
            set => Localizer.SetLanguage(value);
        }

        [SerializeField] protected bool _isTosAccepted;

        /// <summary>
        /// Is Terms Of Service accepted?
        /// </summary>
        public bool IsTosAccepted
        {
            get => _isTosAccepted;
            set
            {
                _isTosAccepted = value;
                Save();
            }
        }

        [SerializeField] protected bool _isDebugFlag;

        /// <summary>
        /// Debug FLag for debugging and diagnostics purposes.
        /// </summary>
        /// <remarks>
        /// Should be hidden in final product.
        /// </remarks>
        public bool IsDebugFlag
        {
            get => _isDebugFlag;
            set
            {
                _isDebugFlag = value;
                Save();
            }
        }

        public string GetPlayerInfoLabel()
        {
            if (ClanId > 0)
            {
                var clan = Storefront.Get().GetClanModel(ClanId);
                if (clan != null)
                {
                    return $"{PlayerName}[{clan.Tag}] {CharacterModel.Name}";
                }
            }
            return $"{PlayerName} {CharacterModel.Name}";
        }
        /// <summary>
        /// Protected <c>Save</c> method to handle single property change.
        /// </summary>
        protected virtual void Save()
        {
            // Placeholder for actual implementation in derived class.
        }

        /// <summary>
        /// Public <c>BatchSave</c> method to save several properties at once.
        /// </summary>
        /// <param name="saveSettings">The action to save all properties in one go.</param>
        public virtual void BatchSave(Action saveSettings)
        {
            // Placeholder for actual implementation in derived class.
        }

        public void DebugResetPlayer()
        {
            // Actually can not delete at this level - just invalidate everything!
            BatchSave(() =>
            {
                PlayerName = string.Empty;
                CharacterModelId = -1;
                ClanId = -1;
                IsTosAccepted = false;
            });
        }

        public override string ToString()
        {
            // This is required for actual implementation to detect changes in our changeable properties!
            return
                $"Name {PlayerName}, Model {CharacterModelId}, Clan {ClanId}, ToS {(IsTosAccepted ? 1 : 0)}, Lang {Language}, Guid {PlayerGuid}";
        }
    }

    /// <summary>
    /// <c>PlayerDataCache</c> implementation using UNITY <c>PlayerPrefs</c> as backing storage.
    /// </summary>
    public class PlayerDataCacheLocal : PlayerDataCache
    {
        private const string PlayerNameKey = "PlayerData.PlayerName";
        private const string PlayerGuidKey = "PlayerData.PlayerGuid";
        private const string CharacterModelIdKey = "PlayerData.CustomCharacterModelId";
        private const string ClanIdKey = "PlayerData.ClanId";
        private const string TermsOfServiceKey = "PlayerData.TermsOfService";
        private const string IsDebugFlagKey = "PlayerData.IsDebugFlag";

        private bool _isBatchSave;

        public PlayerDataCacheLocal()
        {
            _playerName = PlayerPrefs.GetString(PlayerNameKey, string.Empty);
            _characterModelId = PlayerPrefs.GetInt(CharacterModelIdKey, -1);
            _clanId = PlayerPrefs.GetInt(ClanIdKey, -1);
            _playerGuid = PlayerPrefs.GetString(PlayerGuidKey, string.Empty);
            if (string.IsNullOrWhiteSpace(PlayerGuid))
            {
                _playerGuid = CreatePlayerHandle();
                // Save GUID immediately on this device!
                PlayerPrefs.SetString(PlayerGuidKey, PlayerGuid);
                PlayerPrefs.Save();
            }
            _isTosAccepted = PlayerPrefs.GetInt(TermsOfServiceKey, 0) == 1;
            _isDebugFlag = PlayerPrefs.GetInt(IsDebugFlagKey, 0) == 1;
        }

        private static string CreatePlayerHandle()
        {
            // Create same GUID for same device if possible
            // - guid can be used to identify third party cloud game services
            // - we want to keep it constant for single device even this data is wiped e.g. during testing
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            if (deviceId == SystemInfo.unsupportedIdentifier)
            {
                return Guid.NewGuid().ToString();
            }
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.Unicode.GetBytes(deviceId));
                return new Guid(hash).ToString();
            }
        }

        protected override void Save()
        {
            InternalSave();
        }

        public override void BatchSave(Action saveSettings)
        {
            _isBatchSave = true;
            saveSettings?.Invoke();
            _isBatchSave = false;
            InternalSave();
            // Writes all modified preferences to disk.
            PlayerPrefs.Save();
        }

        private void InternalSave()
        {
            if (_isBatchSave)
            {
                return; // Defer saving until later
            }
            // By default Unity writes preferences to disk during OnApplicationQuit().
            // - you can force them to disk using PlayerPrefs.Save().
            PlayerPrefs.SetString(PlayerNameKey, PlayerName);
            PlayerPrefs.SetInt(CharacterModelIdKey, CharacterModelId);
            PlayerPrefs.SetInt(ClanIdKey, ClanId);
            PlayerPrefs.SetString(PlayerGuidKey, PlayerGuid);
            PlayerPrefs.SetInt(TermsOfServiceKey, IsTosAccepted ? 1 : 0);
            PlayerPrefs.SetInt(IsDebugFlagKey, IsDebugFlag ? 1 : 0);
        }
    }
}