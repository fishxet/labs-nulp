package data;

import client.Client;
import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;
import java.io.*;
import java.util.List;

public class ClientDataManager {
    private static final Gson gson = new Gson();

    public static void saveClients(List<Client> clients, String path) throws IOException {
        try (FileWriter writer = new FileWriter(path)) {
            gson.toJson(clients, writer);
        }
    }

    public static List<Client> loadClients(String path) throws IOException {
        try (FileReader reader = new FileReader(path)) {
            TypeToken<List<Client>> typeToken = new TypeToken<>() {};
            return gson.fromJson(reader, typeToken.getType());
        }
    }
}