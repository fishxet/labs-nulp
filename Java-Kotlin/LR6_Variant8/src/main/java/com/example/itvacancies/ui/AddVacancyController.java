package com.example.itvacancies.ui;

import com.example.itvacancies.data.Repository;
import com.example.itvacancies.model.Vacancy;
import javafx.fxml.FXML;
import javafx.scene.control.TextArea;
import javafx.scene.control.TextField;
import javafx.stage.Stage;

public class AddVacancyController {
    @FXML private TextField companyField;
    @FXML private TextField positionField;
    @FXML private TextField technologyField;
    @FXML private TextArea requirementsArea;
    @FXML private TextField salaryField;

    private Repository repo;
    private MainController parent;
    private Vacancy editing;

    public void setRepo(Repository repo) { this.repo = repo; }
    public void setParent(MainController parent) { this.parent = parent; }
    public void setVacancy(Vacancy v) {
        this.editing = v;
        companyField.setText(v.getCompany());
        positionField.setText(v.getPosition());
        technologyField.setText(v.getTechnology());
        requirementsArea.setText(v.getRequirements());
        salaryField.setText(String.valueOf(v.getSalary()));
    }

    @FXML
    private void onSave() {
        String comp = companyField.getText();
        String pos = positionField.getText();
        String tech = technologyField.getText();
        String req = requirementsArea.getText();
        double sal = Double.parseDouble(salaryField.getText());
        if (editing == null) {
            repo.add(new Vacancy(comp, pos, tech, req, sal));
        } else {
            repo.update(editing.getId(), new Vacancy(comp, pos, tech, req, sal));
        }
        ((Stage)companyField.getScene().getWindow()).close();
    }
}