# UI Electronic Assembly
electronic-assembly-window-title = Електронна Плата
electronic-assembly-window-rename = [Перейменувати]
electronic-assembly-window-save = [Зберегти]
electronic-assembly-window-remove-battery = [Дістати]
electronic-assembly-window-power = Живлення: {$charge}/{$max}Дж
electronic-assembly-window-power-no-cell = Живлення: Немає Батареї
electronic-assembly-window-stats = Місце: {$size}/{$maxSize} | Складність: {$complexity}/{$maxComplexity}
electronic-assembly-window-space-stats = Місце: {$size}/{$maxSize}
electronic-assembly-window-complexity-stats = Складність: {$complexity}/{$maxComplexity}

electronic-assembly-window-components-list = Встановлені Компоненти:
electronic-assembly-window-eject = [Витягти]
electronic-assembly-window-inputs = --- Входи ---
electronic-assembly-window-stats-header = --- Статистика ---
electronic-assembly-window-outputs = --- Виходи ---
electronic-assembly-window-activators = --- Активатори ---
electronic-assembly-window-select-prompt = Виберіть компонент зі списку.
electronic-assembly-window-stat-size = Розмір: {$size}
electronic-assembly-window-stat-complexity = Складність: {$complexity}
electronic-assembly-window-stat-power-idle = Живлення (Очік.): {$power}Вт
electronic-assembly-window-stat-power-act = Живлення (Акт.): {$power}Вт
electronic-assembly-window-pin-tooltip = Натисніть З'єднувачем щоб підключити. Натисніть Дебагером щоб вписати дані.

# UI Circuit Printer
circuit-printer-window-title = Принтер Інтегральних Схем
circuit-printer-window-materials = Матеріали: 
circuit-printer-window-materials-none = Жодних
circuit-printer-window-circuits-available = Доступні схеми: 
circuit-printer-window-upgraded-regular = Звичайні
circuit-printer-window-upgraded-advanced = Удосконалені
circuit-printer-window-categories = Категорії:

# UI Circuit Debugger
circuit-debugger-window-title = Дебагер Схем
circuit-debugger-window-current-memory = Поточна пам'ять:
circuit-debugger-window-select-mode = Виберіть режим:
circuit-debugger-window-mode-string = Рядок
circuit-debugger-window-mode-number = Число
circuit-debugger-window-mode-ref = Посилання
circuit-debugger-window-mode-null = Null
circuit-debugger-window-save = Зберегти
circuit-debugger-window-ref-prompt = Натисніть на об'єкт у світі...
circuit-debugger-window-input-placeholder = Введіть дані...

# UI Assembly Interact
assembly-interact-window-title = Вибір Інтерфейсу
assembly-interact-window-prompt = Виберіть компонент для взаємодії:
assembly-interact-window-search-placeholder = Пошук...

# Tools System
circuit-wirer-cancel-wire = Ви скасували підключення.
circuit-wirer-cancel-unwire = Ви скасували відключення.
circuit-wirer-select-wire = Ви вибрали пін. Натисніть на інший пін щоб їх підключити.
circuit-wirer-same-pin = Ви не можете підключити пін до самого себе.
circuit-wirer-connected = Піни успішно підключені.
circuit-wirer-connect-failed = Не вдалося підключити вибрані піни.
circuit-wirer-select-unwire = Ви вибрали пін. Натисніть на інший пін щоб відключити їх.
circuit-wirer-disconnected = Піни успішно від'єднано.
circuit-wirer-disconnect-failed = Не вдалося від'єднати вибрані піни.

circuit-debugger-pulse = Ви надіслали імпульс до цього піна активатора.
circuit-debugger-write-success = Ви вписали дані до цього піна.
circuit-debugger-write-failed = Не вдалося вписати дані до цього піна.

# Entities - Integrated Electronics
ent-BaseIntegratedCircuit = інтегральна схема
ent-BaseElectronicAssembly = електронна плата
ent-ElectronicAssemblySmall = мала електронна плата
ent-ElectronicAssemblyMedium = середня електронна плата
ent-ElectronicAssemblyLarge = велика електронна плата

ent-CircuitWirer = з'єднувач схем
    .desc = Інструмент для з'єднання пінів інтегральних схем.

ent-CircuitDebugger = дебагер схем
    .desc = Дозволяє перевіряти або вписувати дані в піни схем.

ent-IntegratedCircuitPrinter = принтер інтегральних схем
    .desc = Друкує різноманітні інтегральні схеми та електронні компоненти.

# Entities - Circuits
ent-CircuitButton = кнопка
    .desc = Надсилає імпульс, коли гравець використовує закритий корпус в руці.

ent-CircuitSpeaker = динамік
    .desc = Відтворює текст із вхідного піна, коли отримує імпульс.
