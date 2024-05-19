using BepInEx;
using SpaceCraft;
using BepInEx.Logging;
using HarmonyLib;

namespace OxygenFix
{
	[BepInPlugin("TheIllusion.ThePlanetCrafterMods.OxygenFix", BuildInfo.Name, BuildInfo.Version)]
	public class OxygenFix : BaseUnityPlugin
	{
		public static ManualLogSource logger;
		public static float _underWaterOxygenChangeValuePerSec = -3.6f;
		public static bool _playerHaveOxygenRebreather = false;

		public void Awake()
		{
			logger = Logger;
			Harmony.CreateAndPatchAll(typeof(OxygenFix));
		}

		public void Update()
		{
			if (Managers.GetManager<PlayersManager>() != null && Managers.GetManager<PlayersManager>().GetActivePlayerController() != null)
			{
				PlayerGaugesHandler playerGaugesHandler = Managers.GetManager<PlayerGaugesHandler>();

				if (playerGaugesHandler != null)
				{
					_playerHaveOxygenRebreather = (bool)typeof(PlayerGaugesHandler).GetField("_playerHaveOxygenRebreather").GetValue(playerGaugesHandler);
					_underWaterOxygenChangeValuePerSec = (float)typeof(PlayerGaugesHandler).GetField("_underWaterOxygenChangeValuePerSec").GetValue(playerGaugesHandler);
				}
			}
		}

		[HarmonyPatch(typeof(GaugesConsumptionHandler), nameof(GaugesConsumptionHandler.GetOxygenConsumptionRate))]
		[HarmonyPrefix]
		static bool GaugesConsumptionHandler_GetOxygenConsumptionRate(ref float __result)
		{
			if (Managers.GetManager<PlayersManager>() != null && Managers.GetManager<PlayersManager>().GetActivePlayerController() != null)
			{
				WaterHandler waterHandler = Managers.GetManager<WaterHandler>();
				if (waterHandler is null) return true;
				if (waterHandler.IsUnderWater(Managers.GetManager<PlayersManager>().GetActivePlayerController().gameObject.transform.position))
				{
					if (_playerHaveOxygenRebreather)
					{
						__result = (_underWaterOxygenChangeValuePerSec / 2) * Managers.GetManager<GameSettingsHandler>().GetCurrentGameSettings().GetModifierGaugeDrain();
						return false;
					}
					__result = _underWaterOxygenChangeValuePerSec * Managers.GetManager<GameSettingsHandler>().GetCurrentGameSettings().GetModifierGaugeDrain();
					return false;
				}
			}
			return true;
		}
	}
}
