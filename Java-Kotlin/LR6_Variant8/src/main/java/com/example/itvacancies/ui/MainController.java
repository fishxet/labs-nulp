package com.example.itvacancies.ui;

import com.example.itvacancies.data.DataBaseConnector;
import com.example.itvacancies.data.DataBaseRepository;
import com.example.itvacancies.data.Repository;
import com.example.itvacancies.model.Vacancy;
import javafx.collections.FXCollections;
import javafx.collections.ObservableList;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.fxml.FXMLLoader;
import javafx.fxml.Initializable;
import javafx.scene.Scene;
import javafx.scene.control.ComboBox;
import javafx.scene.control.ListView;
import javafx.stage.Modality;
import javafx.stage.Stage;

import java.io.IOException;
import java.net.URL;
import java.util.List;
import java.util.ResourceBundle;
import java.util.stream.Collectors;

public class MainController implements Initializable {
    @FXML private ListView<Vacancy> listVacancies;
    @FXML private ComboBox<String> techCombo;
    @FXML private ComboBox<String> companyCombo;

    private Repository repo;

    @Override
    public void initialize(URL location, ResourceBundle resources) {
        repo = new DataBaseRepository(new DataBaseConnector("LR6_Variant8"));
        loadFilters();
        loadList(repo.getAll());
    }

    private void loadFilters() {
        List<String> techs = repo.getAll().stream()
                .map(Vacancy::getTechnology)
                .distinct()
                .sorted()
                .collect(Collectors.toList());
        techs.add(0, "All");
        techCombo.setItems(FXCollections.observableList(techs));
        techCombo.getSelectionModel().selectFirst();

        List<String> companies = repo.getAll().stream()
                .map(Vacancy::getCompany)
                .distinct()
                .sorted()
                .collect(Collectors.toList());
        companies.add(0, "All");
        companyCombo.setItems(FXCollections.observableList(companies));
        companyCombo.getSelectionModel().selectFirst();
    }

    private void loadList(List<Vacancy> items) {
        ObservableList<Vacancy> obs = FXCollections.observableList(items);
        listVacancies.setItems(obs);
    }

    @FXML
    private void filterByTech(ActionEvent e) {
        String sel = techCombo.getValue();
        if (sel.equals("All")) loadList(repo.getAll());
        else loadList(repo.getByTechnology(sel));
    }

    @FXML
    private void filterByCompany(ActionEvent e) {
        String sel = companyCombo.getValue();
        if (sel.equals("All")) loadList(repo.getAll());
        else loadList(repo.getByCompany(sel));
    }

    @FXML
    private void onAdd(ActionEvent e) throws IOException {
        openForm(null);
    }

    @FXML
    private void onEdit(ActionEvent e) throws IOException {
        Vacancy sel = listVacancies.getSelectionModel().getSelectedItem();
        if (sel != null) openForm(sel);
    }

    @FXML
    private void onDelete(ActionEvent e) {
        Vacancy sel = listVacancies.getSelectionModel().getSelectedItem();
        if (sel != null) {
            repo.delete(sel.getId());
            refreshAll();
        }
    }

    private void openForm(Vacancy edit) throws IOException {
        FXMLLoader loader = new FXMLLoader(getClass().getResource("/com/example/itvacancies/add-vacancy-form.fxml"));
        Stage stage = new Stage();
        stage.initModality(Modality.APPLICATION_MODAL);
        stage.setScene(new Scene(loader.load()));
        AddVacancyController ctl = loader.getController();
        ctl.setRepo(repo);
        ctl.setParent(this);
        if (edit != null) ctl.setVacancy(edit);
        stage.showAndWait();
        refreshAll();
    }

    public void refreshAll() {
        loadFilters();
        loadList(repo.getAll());
    }
}