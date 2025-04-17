using System;
using System.Collections.Generic;

namespace ScheduleGA
{
    public class Lesson
    {
        public string Class { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Teacher { get; set; } = "";
        public string Room { get; set; } = "";
        public int Day { get; set; }
        public int TimeSlot { get; set; }
    }

    public class ScheduleGA
    {
        private static Random _random = new();
        private static List<string> _rooms = new() { "101", "102", "Спортзал" };

        public static void Main()
        {
            var schedule = GenerateSchedule();
            PrintSchedule(schedule);
        }

        private static List<Lesson> GenerateSchedule()
        {
            var lessons = new List<Lesson>();
            // Пример: 5 уроков
            for (int i = 0; i < 5; i++)
            {
                lessons.Add(new Lesson
                {
                    Class = "1A",
                    Subject = "Математика",
                    Teacher = "Іванова",
                    Room = _rooms[_random.Next(_rooms.Count)],
                    Day = _random.Next(1, 6),
                    TimeSlot = _random.Next(1, 6)
                });
            }
            return lessons;
        }

        private static void PrintSchedule(List<Lesson> lessons)
        {
            Console.WriteLine("Расписание:");
            Console.WriteLine("Класс | Предмет | Учитель | Аудитория | День | Время");
            foreach (var lesson in lessons)
            {
                Console.WriteLine($"{lesson.Class} | {lesson.Subject} | {lesson.Teacher} | {lesson.Room} | {lesson.Day} | {lesson.TimeSlot}");
            }
        }
    }
}