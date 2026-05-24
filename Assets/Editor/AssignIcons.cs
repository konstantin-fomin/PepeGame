// AssignIcons.cs
// Одноразовый Editor-скрипт: назначает иконки апгрейдам в SlotBranchConfig.
// Запуск: Tools → Assign Icons
// После успешного запуска можно удалить.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class AssignIcons
{
    [MenuItem("Tools/Assign Icons")]
    public static void Run()
    {
        // 1. Загрузить иконки из папок
        Dictionary<string, Sprite> clickIcons = LoadIcons("Assets/UI/Icons/CLICK");
        Dictionary<string, Sprite> passiveIcons = LoadIcons("Assets/UI/Icons/PASSIVE");
        Dictionary<string, Sprite> specialIcons = LoadIcons("Assets/UI/Icons/SPECIAL");

        // Алиас: у кофейной иконки нет стандартного разделителя " — " в имени файла
        AddAlias(specialIcons, "Бесплатный_кофе___10_минут_ты___машина", "Бесплатный кофе");

        Debug.Log($"[AssignIcons] Иконки загружены — CLICK:{clickIcons.Count} PASSIVE:{passiveIcons.Count} SPECIAL:{specialIcons.Count}");
        LogKeys("CLICK", clickIcons);
        LogKeys("PASSIVE", passiveIcons);
        LogKeys("SPECIAL", specialIcons);

        // 2. Маппинг: configName → список (upgradeTitle, iconKey)
        Dictionary<string, List<(string title, string icon)>> mapping = BuildMapping();

        // 3. Загрузить все SlotBranchConfig и назначить иконки
        string[] guids = AssetDatabase.FindAssets("t:SlotBranchConfig", new[] { "Assets/GameData/Slots" });
        int assigned = 0;
        int errors = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SlotBranchConfig config = AssetDatabase.LoadAssetAtPath<SlotBranchConfig>(path);
            if (config == null) continue;

            if (!mapping.TryGetValue(config.name, out List<(string title, string icon)> entries))
            {
                Debug.LogWarning($"[AssignIcons] Нет маппинга для: {config.name}");
                continue;
            }

            // slotType: 0=Click, 1=Passive, 2=Special
            int slotType = (int)config.slotType;
            Dictionary<string, Sprite> icons = slotType == 0 ? clickIcons
                                              : slotType == 1 ? passiveIcons
                                              : specialIcons;

            foreach ((string title, string iconKey) in entries)
            {
                Upgrade upgrade = config.upgrades.Find(u => u.title == title);
                if (upgrade == null)
                {
                    Debug.LogError($"[AssignIcons] Апгрейд не найден: '{title}' в {config.name}");
                    errors++;
                    continue;
                }

                string normalizedIcon = NormalizeQuotes(iconKey);
                if (!icons.TryGetValue(normalizedIcon, out Sprite sprite))
                {
                    Debug.LogError($"[AssignIcons] Иконка не найдена: '{iconKey}' для '{title}' в {config.name}");
                    errors++;
                    continue;
                }

                upgrade.icon = sprite;
                assigned++;
            }

            EditorUtility.SetDirty(config);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[AssignIcons] Готово! Назначено: {assigned}, Ошибок: {errors}");
    }

    private static Dictionary<string, Sprite> LoadIcons(string folderPath)
    {
        Dictionary<string, Sprite> result = new Dictionary<string, Sprite>();
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Сначала пробуем загрузить как Sprite напрямую
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            // Если не вышло — ищем Sprite среди суб-ассетов
            if (sprite == null)
            {
                Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (Object obj in subAssets)
                {
                    if (obj is Sprite s)
                    {
                        sprite = s;
                        break;
                    }
                }
            }

            if (sprite == null)
            {
                Debug.LogWarning($"[AssignIcons] Не удалось загрузить спрайт: {path}");
                continue;
            }

            string fileName = Path.GetFileNameWithoutExtension(path);

            // Извлекаем ключ: часть до " — " (пробел + em dash + пробел)
            int sep = fileName.IndexOf(" — ");
            if (sep < 0) sep = fileName.IndexOf(" - ");

            string key = sep >= 0 ? fileName.Substring(0, sep).Trim() : fileName.Trim();
            key = NormalizeQuotes(key);

            if (!result.ContainsKey(key))
            {
                result[key] = sprite;
            }
        }

        return result;
    }

    private static string NormalizeQuotes(string s)
    {
        return s.Replace('“', '"')   // левая типографская кавычка
                .Replace('”', '"')   // правая типографская кавычка
                .Replace('«', '"')   // левая ёлочка «
                .Replace('»', '"');   // правая ёлочка »
    }

    private static void AddAlias(Dictionary<string, Sprite> icons, string existingKey, string aliasKey)
    {
        string normalized = NormalizeQuotes(existingKey);
        if (icons.TryGetValue(normalized, out Sprite sprite))
        {
            icons[NormalizeQuotes(aliasKey)] = sprite;
        }
        else
        {
            Debug.LogWarning($"[AssignIcons] Источник алиаса не найден: '{existingKey}'");
        }
    }

    private static void LogKeys(string label, Dictionary<string, Sprite> icons)
    {
        foreach (KeyValuePair<string, Sprite> kv in icons)
        {
            Debug.Log($"  {label}: [{kv.Key}]");
        }
    }

    private static Dictionary<string, List<(string title, string icon)>> BuildMapping()
    {
        return new Dictionary<string, List<(string title, string icon)>>
        {
            // ==================== CLICK ====================

            ["Intern_Click"] = new List<(string, string)>
            {
                ("Горячие ладошки", "Горящие клавиши"),
                ("Старая мембранка", "Нажать ещё раз"),
                ("Подглядел у соседа", "Скопировать пример"),
                ("Тренировка в Excel", "Ctrl+C  Ctrl+V"),
                ("Работа на энтузиазме", "Сделать вид, что понял"),
            },

            ["Junior_Click"] = new List<(string, string)>
            {
                ("Механическая клавиатура", "Горящие клавиши"),
                ("Горячие клавиши", "Ctrl+C  Ctrl+V"),
                ("IDE с плагинами", "Гугл знает лучше"),
                ("Второй монитор", "Дёргать мышь увереннее"),
                ("Ночная сборка", "Горячо исправить"),
            },

            ["Middle_Click"] = new List<(string, string)>
            {
                ("Чистая архитектура", "Костыль, но работает"),
                ("Рефакторинг", "Горячо исправить"),
                ("Паттерны", "Скопировать пример"),
                ("Опыт ошибок", "Нажать ещё раз"),
                ("Чтение чужого кода", "Спросить в чате"),
            },

            ["Senior_Click"] = new List<(string, string)>
            {
                ("Архитекторское чутьё", "Сделать вид, что понял"),
                ("Чтение логов взглядом", "Гугл знает лучше"),
                ("Опыт боли", "Костыль, но работает"),
                ("Менторство", "Спросить в чате"),
                ("Интуиция", "Дёргать мышь увереннее"),
            },

            ["Lead_Click"] = new List<(string, string)>
            {
                ("Стратегическое мышление", "Сделать вид, что понял"),
                ("Делегирование", "Скопировать пример"),
                ("Управление хаосом", "Горячо исправить"),
                ("Системное мышление", "Гугл знает лучше"),
                ("Контроль процессов", "Нажать ещё раз"),
            },

            ["CEO_Click"] = new List<(string, string)>
            {
                ("Видение", "Дёргать мышь увереннее"),
                ("Решения на миллионы", "Горящие клавиши"),
                ("Абсолютный контроль", "Костыль, но работает"),
                ("Мышление масштабами", "Ctrl+C  Ctrl+V"),
                ("Последнее слово", "Горячо исправить"),
            },

            // ==================== PASSIVE ====================

            ["Intern_Passive"] = new List<(string, string)>
            {
                ("Фоновая суета", "Фоновая тревожность"),
                ("Открытый StackOverflow", "Тихо в почте"),
                ("Чек-лист стажёра", "Таска в статусе \"в работе\""),
                ("Шаблон отчёта", "Рабочий чат открыт"),
                ("Автосохранение", "Авто-проверка"),
            },

            ["Junior_Passive"] = new List<(string, string)>
            {
                ("Асинхронные задачи", "Ожидание ответа"),
                ("CI пайплайн", "Авто-проверка"),
                ("Автотесты", "Никто не ответил"),
                ("Документация", "Никто не спрашивает"),
                ("Code snippets", "Рабочий чат открыт"),
            },

            ["Middle_Passive"] = new List<(string, string)>
            {
                ("Оптимизация процессов", "Ожидание ревью"),
                ("Автоматизация рутины", "Авто-проверка"),
                ("Мониторинг", "Фоновая тревожность"),
                ("Логи и алерты", "Никто не ответил"),
                ("Надёжный пайплайн", "Кажется, забыли"),
            },

            ["Senior_Passive"] = new List<(string, string)>
            {
                ("Стабильная система", "Никто не спрашивает"),
                ("Автооптимизация", "Авто-проверка"),
                ("Зрелый продукт", "Ожидание ревью"),
                ("Предсказуемость", "Тихо в почте"),
                ("Надёжность", "Кажется, забыли"),
            },

            ["Lead_Passive"] = new List<(string, string)>
            {
                ("Самоорганизация команды", "Никто не спрашивает"),
                ("Процессы на рельсах", "Таска в статусе \"в работе\""),
                ("Автономные отделы", "Ожидание ответа"),
                ("Масштабирование", "Фоновая тревожность"),
                ("Устойчивость", "Кажется, забыли"),
            },

            ["CEO_Passive"] = new List<(string, string)>
            {
                ("Автономная корпорация", "Никто не спрашивает"),
                ("Экосистема", "Рабочий чат открыт"),
                ("Глобальные процессы", "Ожидание ревью"),
                ("Саморегуляция", "Авто-проверка"),
                ("Постоянный рост", "Тихо в почте"),
            },

            // ==================== SPECIAL ====================

            ["Intern_Special"] = new List<(string, string)>
            {
                ("Бесплатный кофе", "Бесплатный кофе"),
                ("Похвала от тимлида", "Сеньор помог"),
                ("Работа из дома", "Сегодня без созвонов"),
                ("Никто не смотрит", "Менеджер ушёл"),
                ("Дедлайн завтра", "Всем срочно надо"),
            },

            ["Junior_Special"] = new List<(string, string)>
            {
                ("Code Review Boost", "Случайно сделал правильно"),
                ("Синк команды", "Это не моя зона ответственности"),
                ("Хакатон", "Всем срочно надо"),
                ("Продакшн горит", "Прод не упал"),
                ("Бонус от заказчика", "Оно само починилось"),
            },

            ["Middle_Special"] = new List<(string, string)>
            {
                ("Фокус", "Сегодня без созвонов"),
                ("Отпуск тимлида", "Менеджер ушёл"),
                ("Технический долг списан", "Оно само починилось"),
                ("Новый релиз", "Прод не упал"),
                ("Работа в потоке", "Это срочно, но не сейчас"),
            },

            ["Senior_Special"] = new List<(string, string)>
            {
                ("Глубокий фокус", "Сегодня без созвонов"),
                ("Авторитет", "Сеньор помог"),
                ("Кризис менеджмент", "Всем срочно надо"),
                ("Большой релиз", "Прод не упал"),
                ("Поток мастера", "Случайно сделал правильно"),
            },

            ["Lead_Special"] = new List<(string, string)>
            {
                ("Боевой режим", "Всем срочно надо"),
                ("Ключевой дедлайн", "Это срочно, но не сейчас"),
                ("Кризисный штаб", "Это не моя зона ответственности"),
                ("Сверхфокус", "Сегодня без созвонов"),
                ("Рывок", "Оно само починилось"),
            },

            ["CEO_Special"] = new List<(string, string)>
            {
                ("Режим бога", "Менеджер ушёл"),
                ("Сделка века", "Случайно сделал правильно"),
                ("Глобальный рывок", "Всем срочно надо"),
                ("Абсолютный фокус", "Сегодня без созвонов"),
                ("Финальный аккорд", "Прод не упал"),
            },
        };
    }
}
