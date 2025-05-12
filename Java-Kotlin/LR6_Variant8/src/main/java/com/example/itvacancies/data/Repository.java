package com.example.itvacancies.data;

import com.example.itvacancies.model.Vacancy;
import java.util.List;

public interface Repository {
    List<Vacancy> getAll();
    List<Vacancy> getByTechnology(String tech);
    List<Vacancy> getByCompany(String company);
    Vacancy getById(int id);
    boolean add(Vacancy v);
    boolean update(int id, Vacancy v);
    boolean delete(int id);
}