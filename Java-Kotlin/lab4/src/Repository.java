import java.util.List;

public interface Repository {
    List<Vacancy> getAll();
    Vacancy getById(int id);
    boolean addVacancy(Vacancy vacancy);
    boolean deleteVacancy(int id);
    List<Vacancy> filterByLanguageAndSalary(String language, double minSalary, double maxSalary);
    List<Vacancy> getByCompany(String companyName);
}