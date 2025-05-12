import java.io.Serializable;

public class Vacancy implements Serializable {
    private static final long serialVersionUID = 1L;
    private int id;
    private String companyName;
    private String position;
    private String programmingLanguage;
    private String requirements;
    private double salary;

    // Конструктори
    public Vacancy() {}

    public Vacancy(String companyName, String position, String programmingLanguage, String requirements, double salary) {
        this.companyName = companyName;
        this.position = position;
        this.programmingLanguage = programmingLanguage;
        this.requirements = requirements;
        this.salary = salary;
    }

    // Гетери та сетери
    public int getId() { return id; }
    public void setId(int id) { this.id = id; }
    public String getCompanyName() { return companyName; }
    public void setCompanyName(String companyName) { this.companyName = companyName; }
    public String getPosition() { return position; }
    public void setPosition(String position) { this.position = position; }
    public String getProgrammingLanguage() { return programmingLanguage; }
    public void setProgrammingLanguage(String programmingLanguage) { this.programmingLanguage = programmingLanguage; }
    public String getRequirements() { return requirements; }
    public void setRequirements(String requirements) { this.requirements = requirements; }
    public double getSalary() { return salary; }
    public void setSalary(double salary) { this.salary = salary; }

    @Override
    public String toString() {
        return String.format(
                "ID: %d | Компанія: %s | Позиція: %s | Мова: %s | Зарплата: %.2f",
                id, companyName, position, programmingLanguage, salary
        );
    }
}