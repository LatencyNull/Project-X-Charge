using System;
using System.Collections.Generic;
using System.Globalization;

namespace XboxBatteryMonitor;

public static class Strings
{
	private static string _currentLang = "en";

	public static string CurrentLanguage => _currentLang;

	public static readonly IReadOnlyDictionary<string, string> AvailableLanguages = new Dictionary<string, string>
	{
		{ "en", "English" },
		{ "es", "Español" },
		{ "fr", "Français" },
		{ "de", "Deutsch" },
		{ "pt", "Português" },
		{ "zh", "中文" },
		{ "ru", "Русский" }
	};

	private static readonly Dictionary<string, Dictionary<string, string>> _dict = new()
	{
		{
			"es",
			new Dictionary<string, string>
			{
				{ "GhostLockedTip", "Modo fantasma activo (clics atraviesan al juego)" },
				{ "GhostUnlockedTip", "Modo fantasma desactivado" },
				{ "ThemeDark", "Oscuro" },
				{ "ThemeLight", "Claro" },
				{ "ThemeTransparent", "Transparente" },
				{ "CompactTip", "Gira la rueda del ratón para regular opacidad" },
				{ "Show", "Mostrar" },
				{ "Hide", "Ocultar" },
				{ "Themes", "Temas" },
				{ "Controllers", "Controles" },
				{ "TitleText", "Control Xbox" },
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
				{ "NotFound", "No hay ningún control conectado" },
				{ "NotDetected", "Control no detectado" },
				{ "ConnectedNoData", "Control conectado, sin datos de batería" },
				{ "UsbCharged", "Conectado por USB" },
				{ "UsbCharging", "Conectado por USB (cargando)" },
				{ "Charging", "Cargando" },
				{ "HideUntilLow", "Ocultar hasta batería baja" },
				{ "Charged", "Cargado" },
				{ "NotCharged", "Batería normal" },
				{ "AlreadyRunning", "X-Charge ya se encuentra en ejecución. Revisa la bandeja del sistema (al lado del reloj)." },
				{ "Language", "Idioma" },
				{ "GhostMode", "Modo fantasma (clics atraviesan)" },
				{ "AntiBurnIn", "Protección anti-quemado (Pixel Shift)" },
				{ "GlobalHotKey", "Atajo global (Ctrl+Shift+X)" },
				{ "PollInterval", "Frecuencia de sondeo" },
				{ "Poll1s", "1 segundo (Juegos)" },
				{ "Poll2s", "2 segundos" },
				{ "Poll3s", "3 segundos (Recomendado)" },
				{ "Poll5s", "5 segundos (Ahorro)" },
				{ "HotKeyConflict", "No se pudo registrar el atajo global (Ctrl+Shift+X). Ya está en uso por otra aplicación." }
			}
		},
		{
			"en",
			new Dictionary<string, string>
			{
				{ "GhostLockedTip", "Ghost mode active (clicks pass through to game)" },
				{ "GhostUnlockedTip", "Ghost mode disabled" },
				{ "ThemeDark", "Dark" },
				{ "ThemeLight", "Light" },
				{ "ThemeTransparent", "Transparent" },
				{ "CompactTip", "Scroll mouse wheel to adjust opacity" },
				{ "Show", "Show" },
				{ "Hide", "Hide" },
				{ "Themes", "Themes" },
				{ "Controllers", "Controllers" },
				{ "TitleText", "Xbox Controller" },
				{ "CompactMode", "Compact mode" },
				{ "MinimizeTray", "Minimize to tray" },
				{ "CloseTray", "Close" },
				{ "NormalMode", "Normal mode" },
				{ "Searching", "Searching for controller..." },
				{ "BatteryOptions", "Alarm Level" },
				{ "PlaySounds", "Play sounds" },
				{ "SoundUsb", "USB connect/disconnect sound" },
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
				{ "UsbCharged", "Connected via USB" },
				{ "UsbCharging", "Connected via USB (charging)" },
				{ "Charging", "Charging" },
				{ "HideUntilLow", "Hide until low battery" },
				{ "Charged", "Charged" },
				{ "NotCharged", "Normal battery" },
				{ "AlreadyRunning", "X-Charge is already running. Check the system tray (near the clock)." },
				{ "Language", "Language" },
				{ "GhostMode", "Ghost mode (click-through)" },
				{ "AntiBurnIn", "Anti-burn-in protection (Pixel Shift)" },
				{ "GlobalHotKey", "Global hotkey (Ctrl+Shift+X)" },
				{ "PollInterval", "Polling rate" },
				{ "Poll1s", "1 second (Gaming)" },
				{ "Poll2s", "2 seconds" },
				{ "Poll3s", "3 seconds (Recommended)" },
				{ "Poll5s", "5 seconds (Power saver)" },
				{ "HotKeyConflict", "Could not register global hotkey (Ctrl+Shift+X). It is already in use by another application." }
			}
		},
		{
			"fr",
			new Dictionary<string, string>
			{
				{ "GhostLockedTip", "Mode fantôme actif (les clics passent au jeu)" },
				{ "GhostUnlockedTip", "Mode fantôme désactivé" },
				{ "ThemeDark", "Sombre" },
				{ "ThemeLight", "Clair" },
				{ "ThemeTransparent", "Transparent" },
				{ "CompactTip", "Faites défiler la molette de la souris pour régler l'opacité" },
				{ "Show", "Afficher" },
				{ "Hide", "Masquer" },
				{ "Themes", "Thèmes" },
				{ "Controllers", "Manettes" },
				{ "TitleText", "Manette Xbox" },
				{ "CompactMode", "Mode compact" },
				{ "MinimizeTray", "Réduire dans la zone de notification" },
				{ "CloseTray", "Fermer" },
				{ "NormalMode", "Mode normal" },
				{ "Searching", "Recherche de la manette..." },
				{ "BatteryOptions", "Niveau d'alarme" },
				{ "PlaySounds", "Lire les sons" },
				{ "SoundUsb", "Son lors de la connexion USB" },
				{ "SoundBattery", "Son de batterie faible" },
				{ "StartWindows", "Démarrer avec Windows" },
				{ "StartInTray", "Démarrer dans la zone de notification" },
				{ "Exit", "Quitter" },
				{ "LowBattery", "Batterie faible" },
				{ "ControllerHas", "{0} a {1}% de batterie." },
				{ "Controller", "Manette" },
				{ "NoData", "Aucune donnée" },
				{ "NotConnected", "Non connecté" },
				{ "NotFound", "Aucune manette connectée" },
				{ "NotDetected", "Manette non détectée" },
				{ "ConnectedNoData", "Manette connectée, aucune donnée de batterie" },
				{ "UsbCharged", "Connecté via USB" },
				{ "UsbCharging", "Connecté via USB (en charge)" },
				{ "Charging", "En charge" },
				{ "HideUntilLow", "Masquer jusqu'à batterie faible" },
				{ "Charged", "Chargé" },
				{ "NotCharged", "Batterie normale" },
				{ "AlreadyRunning", "X-Charge est déjà en cours d'exécution. Vérifiez la barre des tâches." },
				{ "Language", "Langue" },
				{ "GhostMode", "Mode fantôme (clics directs)" },
				{ "AntiBurnIn", "Protection anti-marquage (Pixel Shift)" },
				{ "GlobalHotKey", "Raccourci global (Ctrl+Shift+X)" },
				{ "PollInterval", "Taux de rafraîchissement" },
				{ "Poll1s", "1 seconde (Jeux)" },
				{ "Poll2s", "2 secondes" },
				{ "Poll3s", "3 secondes (Recommandé)" },
				{ "Poll5s", "5 secondes (Économie)" },
				{ "HotKeyConflict", "Impossible d'enregistrer le raccourci global (Ctrl+Shift+X). Déjà utilisé." }
			}
		},
		{
			"de",
			new Dictionary<string, string>
			{
				{ "GhostLockedTip", "Geistermodus aktiv (Klicks gehen durch)" },
				{ "GhostUnlockedTip", "Geistermodus deaktiviert" },
				{ "ThemeDark", "Dunkel" },
				{ "ThemeLight", "Hell" },
				{ "ThemeTransparent", "Transparent" },
				{ "CompactTip", "Mausrad drehen, um die Deckkraft anzupassen" },
				{ "Show", "Anzeigen" },
				{ "Hide", "Ausblenden" },
				{ "Themes", "Designs" },
				{ "Controllers", "Controller" },
				{ "TitleText", "Xbox Controller" },
				{ "CompactMode", "Kompaktmodus" },
				{ "MinimizeTray", "Ins Infobereich minimieren" },
				{ "CloseTray", "Schließen" },
				{ "NormalMode", "Normaler Modus" },
				{ "Searching", "Suche nach Controller..." },
				{ "BatteryOptions", "Alarmstufe" },
				{ "PlaySounds", "Töne abspielen" },
				{ "SoundUsb", "Ton beim USB-Anschließen" },
				{ "SoundBattery", "Ton bei schwachem Akku" },
				{ "StartWindows", "Mit Windows starten" },
				{ "StartInTray", "Im Infobereich starten" },
				{ "Exit", "Beenden" },
				{ "LowBattery", "Niedriger Akkustand" },
				{ "ControllerHas", "{0} hat noch {1}% Akku." },
				{ "Controller", "Controller" },
				{ "NoData", "Keine Daten" },
				{ "NotConnected", "Nicht verbunden" },
				{ "NotFound", "Kein Controller verbunden" },
				{ "NotDetected", "Controller nicht erkannt" },
				{ "ConnectedNoData", "Controller verbunden, keine Akkudaten" },
				{ "UsbCharged", "Über USB verbunden" },
				{ "UsbCharging", "Über USB verbunden (lädt)" },
				{ "Charging", "Wird geladen" },
				{ "HideUntilLow", "Ausblenden bis Akku schwach" },
				{ "Charged", "Geladen" },
				{ "NotCharged", "Akku normal" },
				{ "AlreadyRunning", "X-Charge wird bereits ausgeführt. Überprüfen Sie den Infobereich." },
				{ "Language", "Sprache" },
				{ "GhostMode", "Geistermodus (Klicks gehen durch)" },
				{ "AntiBurnIn", "Einbrennschutz (Pixel Shift)" },
				{ "GlobalHotKey", "Globaler Hotkey (Ctrl+Shift+X)" },
				{ "PollInterval", "Abfragerate" },
				{ "Poll1s", "1 Sekunde (Gaming)" },
				{ "Poll2s", "2 Sekunden" },
				{ "Poll3s", "3 Sekunden (Empfohlen)" },
				{ "Poll5s", "5 Sekunden (Stromsparen)" },
				{ "HotKeyConflict", "Globaler Hotkey (Ctrl+Shift+X) konnte nicht registriert werden. Bereits belegt." }
			}
		},
		{
			"pt",
			new Dictionary<string, string>
			{
				{ "GhostLockedTip", "Modo fantasma ativo (cliques passam para o jogo)" },
				{ "GhostUnlockedTip", "Modo fantasma desativado" },
				{ "ThemeDark", "Escuro" },
				{ "ThemeLight", "Claro" },
				{ "ThemeTransparent", "Transparente" },
				{ "CompactTip", "Gire a roda do mouse para ajustar a opacidade" },
				{ "Show", "Mostrar" },
				{ "Hide", "Ocultar" },
				{ "Themes", "Temas" },
				{ "Controllers", "Controles" },
				{ "TitleText", "Controle Xbox" },
				{ "CompactMode", "Modo compacto" },
				{ "MinimizeTray", "Minimizar para a bandeja" },
				{ "CloseTray", "Fechar" },
				{ "NormalMode", "Modo normal" },
				{ "Searching", "Procurando controle..." },
				{ "BatteryOptions", "Nível do alarme" },
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
				{ "ConnectedNoData", "Controle conectado, sem dados de bateria" },
				{ "UsbCharged", "Conectado via USB" },
				{ "UsbCharging", "Conectado via USB (carregando)" },
				{ "Charging", "Carregando" },
				{ "HideUntilLow", "Ocultar até bateria fraca" },
				{ "Charged", "Carregado" },
				{ "NotCharged", "Bateria normal" },
				{ "AlreadyRunning", "O X-Charge já está em execução. Verifique a bandeja do sistema." },
				{ "Language", "Idioma" },
				{ "GhostMode", "Modo fantasma (cliques passam direto)" },
				{ "AntiBurnIn", "Proteção anti-burn-in (Pixel Shift)" },
				{ "GlobalHotKey", "Atalho global (Ctrl+Shift+X)" },
				{ "PollInterval", "Frequência de atualização" },
				{ "Poll1s", "1 segundo (Jogos)" },
				{ "Poll2s", "2 segundos" },
				{ "Poll3s", "3 segundos (Recomendado)" },
				{ "Poll5s", "5 segundos (Economia)" },
				{ "HotKeyConflict", "Não foi possível registrar o atalho global (Ctrl+Shift+X). Já em uso." }
			}
		},
		{
			"zh",
			new Dictionary<string, string>
			{
				{ "GhostLockedTip", "幽灵穿透模式已激活（点击穿透到游戏）" },
				{ "GhostUnlockedTip", "幽灵穿透模式已关闭" },
				{ "ThemeDark", "深色" },
				{ "ThemeLight", "浅色" },
				{ "ThemeTransparent", "透明" },
				{ "CompactTip", "滚动鼠标滚轮调节透明度" },
				{ "Show", "显示" },
				{ "Hide", "隐藏" },
				{ "Themes", "主题" },
				{ "Controllers", "手柄列表" },
				{ "TitleText", "Xbox 手柄" },
				{ "CompactMode", "紧凑模式" },
				{ "MinimizeTray", "最小化到托盘" },
				{ "CloseTray", "关闭" },
				{ "NormalMode", "常规模式" },
				{ "Searching", "正在搜索手柄..." },
				{ "BatteryOptions", "报警电量" },
				{ "PlaySounds", "播放提示音" },
				{ "SoundUsb", "USB 连接/断开提示音" },
				{ "SoundBattery", "低电量警告提示音" },
				{ "StartWindows", "开机自启" },
				{ "StartInTray", "启动时最小化到托盘" },
				{ "Exit", "退出" },
				{ "LowBattery", "低电量警告" },
				{ "ControllerHas", "{0} 剩余电量 {1}%。" },
				{ "Controller", "手柄" },
				{ "NoData", "无数据" },
				{ "NotConnected", "未连接" },
				{ "NotFound", "未连接任何手柄" },
				{ "NotDetected", "未检测到手柄" },
				{ "ConnectedNoData", "手柄已连接，暂无电池数据" },
				{ "UsbCharged", "USB 有线供电" },
				{ "UsbCharging", "USB 供电中（充电中）" },
				{ "Charging", "充电中" },
				{ "HideUntilLow", "平时隐藏，低电量时弹出" },
				{ "Charged", "已充满" },
				{ "NotCharged", "电量正常" },
				{ "AlreadyRunning", "X-Charge 已在运行。请检查系统托盘。" },
				{ "Language", "语言" },
				{ "GhostMode", "幽灵穿透模式（点击穿透到游戏）" },
				{ "AntiBurnIn", "防烧屏像素位移 (Pixel Shift)" },
				{ "GlobalHotKey", "全局热键 (Ctrl+Shift+X)" },
				{ "PollInterval", "轮询频率" },
				{ "Poll1s", "1 秒 (游戏响应)" },
				{ "Poll2s", "2 秒" },
				{ "Poll3s", "3 秒 (推荐)" },
				{ "Poll5s", "5 秒 (省电)" },
				{ "HotKeyConflict", "无法注册全局快捷键 (Ctrl+Shift+X)，已被其他程序占用。" }
			}
		},
		{
			"ru",
			new Dictionary<string, string>
			{
				{ "GhostLockedTip", "Режим призрака активен (клики проходят сквозь)" },
				{ "GhostUnlockedTip", "Режим призрака отключен" },
				{ "ThemeDark", "Темная" },
				{ "ThemeLight", "Светлая" },
				{ "ThemeTransparent", "Прозрачная" },
				{ "CompactTip", "Крутите колесико мыши для регулировки прозрачности" },
				{ "Show", "Показать" },
				{ "Hide", "Скрыть" },
				{ "Themes", "Темы" },
				{ "Controllers", "Геймпады" },
				{ "TitleText", "Геймпад Xbox" },
				{ "CompactMode", "Компактный режим" },
				{ "MinimizeTray", "Свернуть в трей" },
				{ "CloseTray", "Закрыть" },
				{ "NormalMode", "Обычный режим" },
				{ "Searching", "Поиск геймпада..." },
				{ "BatteryOptions", "Порог тревоги" },
				{ "PlaySounds", "Звуки" },
				{ "SoundUsb", "Звук подключения USB" },
				{ "SoundBattery", "Звук низкого заряда" },
				{ "StartWindows", "Запуск с Windows" },
				{ "StartInTray", "Запускать в трее" },
				{ "Exit", "Выход" },
				{ "LowBattery", "Низкий заряд" },
				{ "ControllerHas", "{0}: заряд {1}%." },
				{ "Controller", "Геймпад" },
				{ "NoData", "Нет данных" },
				{ "NotConnected", "Не подключен" },
				{ "NotFound", "Геймпад не подключен" },
				{ "NotDetected", "Геймпад не обнаружен" },
				{ "ConnectedNoData", "Геймпад подключен, нет данных" },
				{ "UsbCharged", "Подключен по USB" },
				{ "UsbCharging", "Подключен по USB (заряжается)" },
				{ "Charging", "Заряжается" },
				{ "HideUntilLow", "Скрывать до низкого заряда" },
				{ "Charged", "Заряжен" },
				{ "NotCharged", "Батарея в норме" },
				{ "AlreadyRunning", "X-Charge уже запущен. Проверьте системный трей." },
				{ "Language", "Язык" },
				{ "GhostMode", "Режим призрака (прозрачен для кликов)" },
				{ "AntiBurnIn", "Защита от выгорания (Pixel Shift)" },
				{ "GlobalHotKey", "Глобальная горячая клавиша (Ctrl+Shift+X)" },
				{ "PollInterval", "Частота опроса" },
				{ "Poll1s", "1 секунда (Игры)" },
				{ "Poll2s", "2 секунды" },
				{ "Poll3s", "3 секунды (Рекомендуется)" },
				{ "Poll5s", "5 секунд (Экономия)" },
				{ "HotKeyConflict", "Не удалось зарегистрировать глобальную горячую клавишу (Ctrl+Shift+X). Она уже занята." }
			}
		}
	};

	static Strings()
	{
		string preferred = AppSettings.Current.Language;
		if (!string.IsNullOrWhiteSpace(preferred) && _dict.ContainsKey(preferred.ToLower()))
		{
			_currentLang = preferred.ToLower();
			return;
		}

		string systemLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();
		if (_dict.ContainsKey(systemLang))
		{
			_currentLang = systemLang;
		}
	}

	public static bool SetLanguage(string langCode)
	{
		string lower = langCode.ToLower();
		if (_dict.ContainsKey(lower))
		{
			_currentLang = lower;
			AppSettings.Current.Language = lower;
			AppSettings.Current.Save();
			return true;
		}
		return false;
	}

	public static string Get(string key)
	{
		if (_dict.TryGetValue(_currentLang, out var currentDict) && currentDict.TryGetValue(key, out string? value) && value != null)
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
