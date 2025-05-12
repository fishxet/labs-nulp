package data;

import client.*;
import com.google.gson.*;
import com.google.gson.reflect.TypeToken;
import java.io.*;
import java.lang.reflect.Type;
import java.util.*;

public class ClientDataManager {
    private static final Gson gson = new GsonBuilder()
            .registerTypeAdapter(Client.class, new ClientTypeAdapter())
            .setPrettyPrinting()
            .create();

    public static void saveClients(List<Client> clients, String path) throws IOException {
        try (FileWriter writer = new FileWriter(path)) {
            gson.toJson(clients, writer);
        }
    }

    public static List<Client> loadClients(String path) throws IOException {
        File file = new File(path);

        if (!file.exists()) {
            return new ArrayList<>();
        }

        try (FileReader reader = new FileReader(file)) {
            // Сначала пробуем прочитать в новом формате
            try {
                Type listType = new TypeToken<List<Client>>(){}.getType();
                List<Client> clients = gson.fromJson(reader, listType);
                return clients != null ? clients : new ArrayList<>();
            } catch (JsonParseException e) {
                // Если не получилось, пробуем старый формат
                reader.close();
                return loadLegacyFormat(file);
            }
        }
    }

    private static List<Client> loadLegacyFormat(File file) throws IOException {
        try (FileReader reader = new FileReader(file)) {
            JsonElement element = JsonParser.parseReader(reader);

            if (!element.isJsonArray()) {
                throw new JsonParseException("Ожидается массив");
            }

            List<Client> clients = new ArrayList<>();
            JsonArray array = element.getAsJsonArray();

            for (JsonElement item : array) {
                try {
                    JsonObject obj = item.getAsJsonObject();
                    Client client = parseLegacyClient(obj);
                    if (client != null) {
                        clients.add(client);
                    }
                } catch (Exception e) {
                    System.err.println("Ошибка парсинга клиента: " + e.getMessage());
                }
            }

            // Автоматически конвертируем в новый формат
            if (!clients.isEmpty()) {
                file.renameTo(new File(file.getAbsolutePath() + ".legacy"));
                saveClients(clients, file.getAbsolutePath());
            }

            return clients;
        }
    }

    private static Client parseLegacyClient(JsonObject obj) {
        if (obj.has("discount")) {
            return new RetailCustomer(
                    getString(obj, "id", ""),
                    getString(obj, "name", ""),
                    getString(obj, "email", ""),
                    getDouble(obj, "discount", 0.0)
            );
        } else if (obj.has("minOrderVolume")) {
            return new WholesaleCustomer(
                    getString(obj, "id", ""),
                    getString(obj, "name", ""),
                    getString(obj, "email", ""),
                    getDouble(obj, "minOrderVolume", 0.0)
            );
        } else if (obj.has("contractNumber")) {
            return new PartnerCompany(
                    getString(obj, "id", ""),
                    getString(obj, "name", ""),
                    getString(obj, "email", ""),
                    getString(obj, "contractNumber", "")
            );
        }
        return null;
    }

    private static String getString(JsonObject obj, String key, String defaultValue) {
        return obj.has(key) ? obj.get(key).getAsString() : defaultValue;
    }

    private static double getDouble(JsonObject obj, String key, double defaultValue) {
        return obj.has(key) ? obj.get(key).getAsDouble() : defaultValue;
    }

    private static class ClientTypeAdapter implements JsonSerializer<Client>, JsonDeserializer<Client> {
        @Override
        public JsonElement serialize(Client src, Type typeOfSrc, JsonSerializationContext context) {
            JsonObject obj = new JsonObject();
            obj.addProperty("type", src.getClass().getSimpleName());
            obj.add("data", context.serialize(src));
            return obj;
        }

        @Override
        public Client deserialize(JsonElement json, Type typeOfT, JsonDeserializationContext context)
                throws JsonParseException {
            try {
                JsonObject obj = json.getAsJsonObject();

                if (!obj.has("type") || !obj.has("data")) {
                    throw new JsonParseException("Отсутствуют обязательные поля");
                }

                String type = obj.get("type").getAsString();
                JsonElement data = obj.get("data");

                switch (type) {
                    case "RetailCustomer":
                        return context.deserialize(data, RetailCustomer.class);
                    case "WholesaleCustomer":
                        return context.deserialize(data, WholesaleCustomer.class);
                    case "PartnerCompany":
                        return context.deserialize(data, PartnerCompany.class);
                    default:
                        throw new JsonParseException("Неизвестный тип клиента: " + type);
                }
            } catch (Exception e) {
                throw new JsonParseException("Ошибка десериализации", e);
            }
        }
    }
}