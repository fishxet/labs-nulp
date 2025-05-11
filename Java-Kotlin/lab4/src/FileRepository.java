import java.io.*;
import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;

public class FileRepository implements Repository {
    private String fileName;
    private List<Vacancy> vacancies;

    public FileRepository(String fileName) {
        this.fileName = fileName;
        this.vacancies = new ArrayList<>();
        loadData();
    }

    private void loadData() {
        try (ObjectInputStream ois = new ObjectInputStream(new FileInputStream(fileName))) {
            vacancies = (List<Vacancy>) ois.readObject();
        } catch (FileNotFoundException e) {
            System.out.println("Файл не знайдено. Створено новий файл.");
        } catch (IOException | ClassNotFoundException e) {
            e.printStackTrace();
        }
    }

    private void saveData() {
        try (ObjectOutputStream oos = new ObjectOutputStream(new FileOutputStream(fileName))) {
            oos.writeObject(vacancies);
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    @Override
    public List<Vacancy> getAll() {
        return new ArrayList<>(vacancies);
    }

    @Override
    public Vacancy getById(int id) {
        return vacancies.stream()
                .filter(v -> v.getId() == id)
                .findFirst()
                .orElse(null);
    }

    @Override
    public boolean addVacancy(Vacancy vacancy) {
        int newId = vacancies.isEmpty() ? 1 : vacancies.get(vacancies.size() - 1).getId() + 1;
        vacancy.setId(newId);
        vacancies.add(vacancy);
        saveData();
        return true;
    }

    @Override
    public boolean deleteVacancy(int id) {
        Vacancy vacancy = getById(id);
        if (vacancy != null) {
            vacancies.remove(vacancy);
            saveData();
            return true;
        }
        return false;
    }

    @Override
    public List<Vacancy> filterByLanguageAndSalary(String language, double minSalary, double maxSalary) {
        return vacancies.stream()
                .filter(v -> v.getProgrammingLanguage().equalsIgnoreCase(language)
                        && v.getSalary() >= minSalary
                        && v.getSalary() <= maxSalary)
                .collect(Collectors.toList());
    }

    @Override
    public List<Vacancy> getByCompany(String companyName) {
        return vacancies.stream()
                .filter(v -> v.getCompanyName().equalsIgnoreCase(companyName))
                .collect(Collectors.toList());
    }
}