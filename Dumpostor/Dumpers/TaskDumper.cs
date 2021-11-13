using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Il2CppSystem.Text;
using UnityEngine;

namespace Dumpostor.Dumpers
{
    public class TaskDumper : IMapDumper
    {
        public enum TaskLength
        {
            Common,
            Long,
            Short
        }

        public string FileName => "tasks.json";

        public string Dump(MapTypes mapType, ShipStatus shipStatus)
        {
            var tasks = new Dictionary<int, TaskInfo>();

            void Handle(NormalPlayerTask task, TaskLength length)
            {
                task.Initialize();

                var taskConsoles = new List<TaskConsole>();

                foreach (var console in shipStatus.AllConsoles.Where(console => console.TaskTypes.Contains(task.TaskType)))
                {
                    taskConsoles.Add(new TaskConsole(console.ConsoleId, console.Room, console.transform.position, console.UsableDistance));
                }

                tasks.Add(task.Index, new TaskInfo(task, length, taskConsoles));
            }

            foreach (var task in shipStatus.CommonTasks)
            {
                Handle(task, TaskLength.Common);
            }

            foreach (var task in shipStatus.NormalTasks)
            {
                Handle(task, TaskLength.Short);
            }

            foreach (var task in shipStatus.LongTasks)
            {
                Handle(task, TaskLength.Long);
            }

            return JsonSerializer.Serialize(tasks.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new Vector2Converter()
                }
            });
        }

        public class TaskInfo
        {
            public string Type { get; }
            public TaskTypes TaskType { get; }
            public string HudText { get; }
            public TaskLength Length { get; }
            public List<TaskConsole> Consoles { get; }

            public TaskInfo(NormalPlayerTask task, TaskLength length, List<TaskConsole> consoles)
            {
                Type = task.GetIl2CppType().Name;
                TaskType = task.TaskType;
                Length = length;
                Consoles = consoles;

                var stringBuilder = new StringBuilder();
                task.AppendTaskText(stringBuilder);
                HudText = stringBuilder.ToString().Replace("<color=#00DD00FF>", "").Replace("<color=#FFFF00FF>", "").Replace("</color>", "").TrimEnd(Environment.NewLine.ToCharArray());
            }
        }

        public class TaskConsole
        {
            public int Id { get; }
            public SystemTypes Room { get; }
            public Vector2 Position { get; }
            public float UsableDistance { get; }

            public TaskConsole(int id, SystemTypes room, Vector2 position, float usableDistance)
            {
                Id = id;
                Room = room;
                Position = position;
                UsableDistance = usableDistance;
            }
        }
    }
}
