package data.model

import kotlinx.serialization.Serializable

@Serializable
data class Country(
    val name: Name,
    val capital: List<String> = emptyList(),
    val flags: Flags,
    val independent: Boolean? = null,
    val region: String? = null,
    val subregion: String? = null,
    val population: Long? = null,
    val languages: Map<String, String>? = null,
    val demonyms: Map<String, Demonym>? = null
)

@Serializable
data class Name(val common: String)

@Serializable
data class Flags(val png: String)

@Serializable
data class Demonym(val f: String?, val m: String?)