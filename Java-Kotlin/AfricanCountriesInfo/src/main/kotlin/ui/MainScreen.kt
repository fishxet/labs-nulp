package ui

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import data.CountriesApiClient
import data.model.Country
import kotlinx.coroutines.launch
import org.jetbrains.skia.Image
import java.net.URL
import androidx.compose.ui.graphics.toComposeImageBitmap
import androidx.compose.foundation.Image as ComposeImage

@Composable
fun MainScreen() {
    var countries by remember { mutableStateOf<List<Country>>(emptyList()) }
    var selected by remember { mutableStateOf<Country?>(null) }
    val scope = rememberCoroutineScope()

    LaunchedEffect(Unit) {
        scope.launch {
            countries = CountriesApiClient.getAfricanCountries()
        }
    }

    if (selected != null) {
        CountryDetail(country = selected!!) { selected = null }
    } else {
        CountryList(countries) { clicked -> selected = clicked }
    }
}

@Composable
fun CountryList(countries: List<Country>, onClick: (Country) -> Unit) {
    LazyColumn(modifier = Modifier.fillMaxSize().padding(16.dp)) {
        items(countries) { country ->
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(8.dp)
                    .clickable { onClick(country) }
            ) {
                // Flag image
                val img = remember(country.flags.png) {
                    Image.makeFromEncoded(URL(country.flags.png).readBytes()).toComposeImageBitmap()
                }
                ComposeImage(bitmap = img, contentDescription = country.name.common, modifier = Modifier.size(40.dp))
                Spacer(Modifier.width(12.dp))
                Column {
                    Text(country.name.common, fontSize = 18.sp, fontWeight = FontWeight.Bold)
                    Text(country.capital.getOrElse(0) { "—" }, fontSize = 14.sp)
                    Text(country.region ?: "", fontSize = 12.sp)
                }
            }
        }
    }
}

@Composable
fun CountryDetail(country: Country, onBack: () -> Unit) {
    Column(modifier = Modifier.fillMaxSize().padding(16.dp)) {
        Text("← Назад", modifier = Modifier
            .clickable { onBack() }
            .padding(8.dp), color = MaterialTheme.colors.primary)
        Spacer(Modifier.height(16.dp))

        // Flag
        val img = remember(country.flags.png) {
            Image.makeFromEncoded(URL(country.flags.png).readBytes()).toComposeImageBitmap()
        }
        ComposeImage(bitmap = img, contentDescription = country.name.common, modifier = Modifier.size(100.dp))
        Spacer(Modifier.height(8.dp))

        Text(country.name.common, fontSize = 24.sp, fontWeight = FontWeight.Bold)
        Spacer(Modifier.height(8.dp))

        Text("Столиця: ${country.capital.getOrElse(0) { "—" }}")
        Text("Регіон: ${country.region ?: "—"}")
        Text("Субрегіон: ${country.subregion ?: "—"}")
        Text("Населення: ${country.population ?: 0}")
        Text("Незалежна: ${if (country.independent == true) "Так" else "Ні"}")
        Text("Мови: ${country.languages?.values?.joinToString(", ") ?: "—"}")
        Text("Демоніми (жін): ${country.demonyms?.get("eng")?.f ?: "—"}")
        Text("Демоніми (чол): ${country.demonyms?.get("eng")?.m ?: "—"}")
    }
}