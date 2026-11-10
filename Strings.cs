using System.Collections.Generic;
using System.Globalization;

namespace XboxBatteryMonitor;

public static class Strings
{
	private static string _currentLang;

	private static readonly Dictionary<string, Dictionary<string, string>> _dict;

	static Strings()
	{
		_currentLang = "en";
		_dict = new Dictionary<string, Dictionary<string, string>>
		{
			{
				"es",
				new Dictionary<string, string>
				{
					{ "Show", "Mostrar" },
					{ "Hide", "Ocultar" },
					{ "Themes", "Temas" },
					{ "Controllers", "Controles" },
					{ "TitleText", "Control Xbox" },
					{ "HoursLeft", "(~{0}h)" },
					{ "CompactMode", "Modo compacto" },
					{ "MinimizeTray", "Minimizar a bandeja" },
					{ "CloseTray", "Cerrar" },
					{ "NormalMode", "Modo normal" },
					{ "Searching", "Buscando control..." },
					{ "BatteryOptions", "Nivel de Alarma" },
					{ "PlaySounds", "Reproducir sonidos" },
					{ "SoundUsb", "Sonido al conectar USB" },
					{ "SoundBattery", "Sonido de batería baja" },
					{ "StartWindows", "Iniciar con Windows" },
					{ "StartInTray", "Iniciar en bandeja del sistema" },
					{ "Exit", "Salir" },
					{ "LowBattery", "Batería baja" },
					{ "ControllerHas", "El {0} tiene {1}% de batería." },
					{ "Controller", "Control" },
					{ "NoData", "Sin datos" },
					{ "NotConnected", "No conectado" },
					{ "NotFound", "No hay ningun control conectado" },
					{ "NotDetected", "Control no detectado" },
					{ "ConnectedNoData", "Control conectado, sin datos de bateria" },
					{ "UsbCharged", "Conectado por USB (cargado)" },
					{ "UsbCharging", "Conectado por USB (cargando)" },
					{ "Charging", "Cargando" },
					{ "HideUntilLow", "Ocultar hasta batería baja" },
					{ "Charged", "Cargado" },
					{ "NotCharged", "Sin cargar" }
				}
			},
			{
				"en",
				new Dictionary<string, string>
				{
					{ "Show", "Show" },
					{ "Hide", "Hide" },
					{ "Themes", "Themes" },
					{ "Controllers", "Controllers" },
					{ "TitleText", "Xbox Controller" },
					{ "HoursLeft", "(~{0}h)" },
					{ "CompactMode", "Compact mode" },
					{ "MinimizeTray", "Minimize to tray" },
					{ "CloseTray", "Close" },
					{ "NormalMode", "Normal mode" },
					{ "Searching", "Searching for controller..." },
					{ "BatteryOptions", "Alarm Level" },
					{ "PlaySounds", "Play sounds" },
					{ "SoundUsb", "USB connect sound" },
					{ "SoundBattery", "Low battery sound" },
					{ "StartWindows", "Start with Windows" },
					{ "StartInTray", "Start in system tray" },
					{ "Exit", "Exit" },
					{ "LowBattery", "Low battery" },
					{ "ControllerHas", "The {0} has {1}% battery left." },
					{ "Controller", "Controller" },
					{ "NoData", "No data" },
					{ "NotConnected", "Not connected" },
					{ "NotFound", "No controller connected" },
					{ "NotDetected", "Controller not detected" },
					{ "ConnectedNoData", "Controller connected, no battery data" },
					{ "UsbCharged", "USB Connected (charged)" },
					{ "UsbCharging", "USB Connected (charging)" },
					{ "Charging", "Charging" },
					{ "HideUntilLow", "Hide until low battery" },
					{ "Charged", "Charged" },
					{ "NotCharged", "Not charging" }
				}
			},
			{
				"fr",
				new Dictionary<string, string>
				{
					{ "Show", "Afficher" },
					{ "Hide", "Masquer" },
					{ "Themes", "Thèmes" },
					{ "Controllers", "Manettes" },
					{ "TitleText", "Manette Xbox" },
					{ "HoursLeft", "(~{0}h)" },
					{ "CompactMode", "Mode compact" },
					{ "MinimizeTray", "Réduire" },
					{ "CloseTray", "Fermer" },
					{ "NormalMode", "Mode normal" },
					{ "Searching", "Recherche de manette..." },
					{ "BatteryOptions", "Niveau d'alarme" },
					{ "PlaySounds", "Jouer les sons" },
					{ "SoundUsb", "Son de connexion USB" },
					{ "SoundBattery", "Son de batterie faible" },
					{ "StartWindows", "Démarrer avec Windows" },
					{ "StartInTray", "Démarrer dans la zone de notification" },
					{ "Exit", "Quitter" },
					{ "LowBattery", "Batterie faible" },
					{ "ControllerHas", "La {0} a {1}% de batterie." },
					{ "Controller", "Manette" },
					{ "NoData", "Pas de données" },
					{ "NotConnected", "Non connecté" },
					{ "NotFound", "Aucune manette connectée" },
					{ "NotDetected", "Manette non détectée" },
					{ "ConnectedNoData", "Manette connectée, pas de données" },
					{ "UsbCharged", "Connecté via USB (chargé)" },
					{ "UsbCharging", "Connecté via USB (en charge)" },
					{ "Charging", "En charge" },
					{ "HideUntilLow", "Masquer jusqu'à batterie faible" },
					{ "Charged", "Chargé" },
					{ "NotCharged", "Ne charge pas" }
				}
			},
			{
				"de",
				new Dictionary<string, string>
				{
					{ "Show", "Anzeigen" },
					{ "Hide", "Ausblenden" },
					{ "Themes", "Designs" },
					{ "Controllers", "Controller" },
					{ "TitleText", "Xbox-Controller" },
					{ "HoursLeft", "(~{0}h)" },
					{ "CompactMode", "Kompaktmodus" },
					{ "MinimizeTray", "Minimieren" },
					{ "CloseTray", "Schließen" },
					{ "NormalMode", "Normaler Modus" },
					{ "Searching", "Suche nach Controller..." },
					{ "BatteryOptions", "Alarmstufe" },
					{ "PlaySounds", "Töne abspielen" },
					{ "SoundUsb", "USB-Verbindungston" },
					{ "SoundBattery", "Ton bei schwacher Batterie" },
					{ "StartWindows", "Mit Windows starten" },
					{ "StartInTray", "Im System-Tray starten" },
					{ "Exit", "Beenden" },
					{ "LowBattery", "Niedriger Batteriestatus" },
					{ "ControllerHas", "Der {0} hat {1}% Batterie." },
					{ "Controller", "Controller" },
					{ "NoData", "Keine Daten" },
					{ "NotConnected", "Nicht verbunden" },
					{ "NotFound", "Kein Controller verbunden" },
					{ "NotDetected", "Controller nicht erkannt" },
					{ "ConnectedNoData", "Verbunden, keine Batteriedaten" },
					{ "UsbCharged", "USB verbunden (aufgeladen)" },
					{ "UsbCharging", "USB verbunden (lädt)" },
					{ "Charging", "Lädt" },
					{ "HideUntilLow", "Ausblenden bis Batterie schwach" },
					{ "Charged", "Aufgeladen" },
					{ "NotCharged", "Lädt nicht" }
				}
			},
			{
				"pt",
				new Dictionary<string, string>
				{
					{ "Show", "Mostrar" },
					{ "Hide", "Ocultar" },
					{ "Themes", "Temas" },
					{ "Controllers", "Controles" },
					{ "TitleText", "Controle Xbox" },
					{ "HoursLeft", "(~{0}h)" },
					{ "CompactMode", "Modo compacto" },
					{ "MinimizeTray", "Minimizar" },
					{ "CloseTray", "Fechar" },
					{ "NormalMode", "Modo normal" },
					{ "Searching", "Buscando controle..." },
					{ "BatteryOptions", "Nível de Alarme" },
					{ "PlaySounds", "Reproduzir sons" },
					{ "SoundUsb", "Som ao conectar USB" },
					{ "SoundBattery", "Som de bateria fraca" },
					{ "StartWindows", "Iniciar com o Windows" },
					{ "StartInTray", "Iniciar na bandeja do sistema" },
					{ "Exit", "Sair" },
					{ "LowBattery", "Bateria fraca" },
					{ "ControllerHas", "O {0} tem {1}% de bateria." },
					{ "Controller", "Controle" },
					{ "NoData", "Sem dados" },
					{ "NotConnected", "Não conectado" },
					{ "NotFound", "Nenhum controle conectado" },
					{ "NotDetected", "Controle não detectado" },
					{ "ConnectedNoData", "Controle conectado, sem dados" },
					{ "UsbCharged", "Conectado por USB (carregado)" },
					{ "UsbCharging", "Conectado por USB (carregando)" },
					{ "Charging", "Carregando" },
					{ "HideUntilLow", "Ocultar até bateria fraca" },
					{ "Charged", "Carregado" },
					{ "NotCharged", "Não carregando" }
				}
			},
			{
				"zh",
				new Dictionary<string, string>
				{
					{ "Show", "显示" },
					{ "Hide", "隐藏" },
					{ "Themes", "主题" },
					{ "Controllers", "控制器" },
					{ "TitleText", "Xbox 控制器" },
					{ "HoursLeft", "(~{0}h)" },
					{ "CompactMode", "精简模式" },
					{ "MinimizeTray", "最小化" },
					{ "CloseTray", "关闭" },
					{ "NormalMode", "正常模式" },
					{ "Searching", "正在寻找控制器..." },
					{ "BatteryOptions", "警报级别" },
					{ "PlaySounds", "播放声音" },
					{ "SoundUsb", "USB 连接提示音" },
					{ "SoundBattery", "低电量提示音" },
					{ "StartWindows", "开机启动" },
					{ "StartInTray", "在系统托盘启动" },
					{ "Exit", "退出" },
					{ "LowBattery", "电量低" },
					{ "ControllerHas", "{0} 还有 {1}% 的电量。" },
					{ "Controller", "控制器" },
					{ "NoData", "无数据" },
					{ "NotConnected", "未连接" },
					{ "NotFound", "未连接控制器" },
					{ "NotDetected", "未检测到控制器" },
					{ "ConnectedNoData", "控制器已连接，无数据" },
					{ "UsbCharged", "USB 已连接 (已充满)" },
					{ "UsbCharging", "USB 已连接 (充电中)" },
					{ "Charging", "充电中" },
					{ "HideUntilLow", "低电量前隐藏" },
					{ "Charged", "已充满" },
					{ "NotCharged", "未充电" }
				}
			},
			{
				"ru",
				new Dictionary<string, string>
				{
					{ "Show", "Показать" },
					{ "Hide", "Скрыть" },
					{ "Themes", "Темы" },
					{ "Controllers", "Контроллеры" },
					{ "TitleText", "Контроллер Xbox" },
					{ "HoursLeft", "(~{0}h)" },
					{ "CompactMode", "Компактный режим" },
					{ "MinimizeTray", "Свернуть" },
					{ "CloseTray", "Закрыть" },
					{ "NormalMode", "Обычный режим" },
					{ "Searching", "Поиск контроллера..." },
					{ "BatteryOptions", "Уровень сигнала" },
					{ "PlaySounds", "Воспроизводить звуки" },
					{ "SoundUsb", "Звук подключения USB" },
					{ "SoundBattery", "Звук низкого заряда" },
					{ "StartWindows", "Запускать с Windows" },
					{ "StartInTray", "Запускать в системном трее" },
					{ "Exit", "Выход" },
					{ "LowBattery", "Низкий заряд батареи" },
					{ "ControllerHas", "{0}: заряд {1}%." },
					{ "Controller", "Контроллер" },
					{ "NoData", "Нет данных" },
					{ "NotConnected", "Не подключен" },
					{ "NotFound", "Контроллер не подключен" },
					{ "NotDetected", "Контроллер не обнаружен" },
					{ "ConnectedNoData", "Контроллер подключен, нет данных" },
					{ "UsbCharged", "Подключен по USB (заряжен)" },
					{ "UsbCharging", "Подключен по USB (заряжается)" },
					{ "Charging", "Заряжается" },
					{ "HideUntilLow", "Скрывать до низкого заряда" },
					{ "Charged", "Заряжен" },
					{ "NotCharged", "Не заряжается" }
				}
			}
		};
		string text = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();
		if (_dict.ContainsKey(text))
		{
			_currentLang = text;
		}
	}

	public static string Get(string key)
	{
		if (_dict[_currentLang].TryGetValue(key, out string? value) && value != null)
		{
			return value;
		}
		if (_dict["en"].TryGetValue(key, out string? value2) && value2 != null)
		{
			return value2;
		}
		return key;
	}
}
