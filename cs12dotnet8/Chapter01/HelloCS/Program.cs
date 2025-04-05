string name = typeof(Program).Namespace ?? "None!";
Console.WriteLine($"Namespace: {name}");
throw new Exception();