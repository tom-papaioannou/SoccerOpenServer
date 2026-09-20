// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using Microsoft.EntityFrameworkCore;
using SoccerOpenServer.Models.Competitions;
using SoccerOpenServer.Models.Contracts;
using SoccerOpenServer.Models.People;
using SoccerOpenServer.Models.Teams;
using SoccerOpenServer.Models.World;
using System;

namespace SoccerOpenServer.Services
{
    public interface ITeamGenerationService
    {
       Task<List<Team>> GenerateTeamsForCompetition(Guid? serverID, Guid? nationID, int numberOfTeams = 16, int priority = 1);
       Task AssignPlayersToGeneratedTeams(IEnumerable<Guid> teamIDs);
       Task AssignPlayersToTactic(Guid tacticID, Guid teamID, Formation? formation);
    }

    public class TeamGenerationService : ITeamGenerationService
    {
        private static readonly Dictionary<string, NationGenerationData> GenerationDataByNation = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Greece"] = new(
                PriorityOneTeamNames:
                [
                    "Athinaikos FC", "Piraeus Harbor", "Thessalia Union", "Patraikos", "Crete Mariners",
                    "Olympia Stars", "Aegean Wave", "Macedonia Eagles", "Epirus Gate", "Sparta Forge",
                    "Rhodes Knights", "Larissa Storm", "Corinth Shield", "Delphi Oracle", "Volos Tide",
                    "Kavala North"
                ],
                PriorityTwoTeamNames:
                [
                    "Attica Rovers", "Pella United", "Achaia Athletic", "Messinia Town", "Thrace Wanderers",
                    "Samos Harbor", "Chios Mariners", "Aitolia FC", "Laconia Stars", "Argos Vale",
                    "Kos Islanders", "Drama Falcons", "Xanthi Forge", "Serres Dynamo", "Preveza Coast",
                    "Trikala Union"
                ],
                FirstNames:
                [
                    "Giorgos", "Yannis", "Nikos", "Dimitris", "Kostas", "Panagiotis", "Vasilis", "Thanasis",
                    "Christos", "Andreas", "Stelios", "Manolis", "Sotiris", "Lefteris", "Michalis", "Spyros",
                    "Petros", "Antonis", "Theodoros", "Alexandros"
                ],
                LastNames:
                [
                    "Papadopoulos", "Nikolaidis", "Georgiou", "Dimitriou", "Konstantinou", "Ioannidis", "Vasileiou", "Christodoulou",
                    "Antonopoulos", "Theodorou", "Stavridis", "Manolakis", "Karagiannis", "Papanikolaou", "Kotsakis", "Roussos",
                    "Mavridis", "Tzimas", "Laskaridis", "Economou"
                ],
                Cities:
                [
                    "Athens", "Piraeus", "Thessaloniki", "Patras", "Heraklion",
                    "Larissa", "Volos", "Ioannina", "Kalamata", "Kavala"
                ]),
            ["England"] = new(
                PriorityOneTeamNames:
                [
                    "Northbridge FC", "London Borough", "Mersey Albion", "Yorkshire County", "Bristol Harbors",
                    "Westford United", "Eastmoor Town", "Kingsport Athletic", "Rivergate FC", "Lancaster Vale",
                    "Stonechester", "Southwick City", "Crown Anchor FC", "Redminster", "Oakfield Rovers",
                    "Brightmere"
                ],
                PriorityTwoTeamNames:
                [
                    "Ashford Town", "Derwent FC", "Midshire Athletic", "Portsmouth Vale", "Chesterfield Rovers",
                    "Blackpool Sands", "Reading Borough", "Trentham United", "Hartford City", "Windsor Albion",
                    "Somerset County", "Rutland FC", "Devonport Mariners", "Sunderland Forge", "Camden Wanderers",
                    "Oxford Heath"
                ],
                FirstNames:
                [
                    "James", "Oliver", "George", "Harry", "Jack", "Charlie", "Thomas", "William",
                    "Henry", "Alfie", "Noah", "Finley", "Joshua", "Daniel", "Samuel", "Edward",
                    "Jacob", "Alexander", "Max", "Joseph"
                ],
                LastNames:
                [
                    "Smith", "Johnson", "Taylor", "Brown", "Wilson", "Evans", "Thomas", "Roberts",
                    "Walker", "White", "Hughes", "Edwards", "Green", "Hall", "Turner", "Carter",
                    "Phillips", "Mitchell", "Baker", "Campbell"
                ],
                Cities:
                [
                    "London", "Manchester", "Liverpool", "Birmingham", "Leeds",
                    "Bristol", "Newcastle", "Sheffield", "Nottingham", "Leicester"
                ]),
            ["Italy"] = new(
                PriorityOneTeamNames:
                [
                    "Roma Aurea", "Milano Navigli", "Torino Bulls", "Napoli Mare", "Firenze Viola",
                    "Genova Lanterns", "Bologna Towers", "Verona Arena", "Parma Ducale", "Palermo Sole",
                    "Bari Levante", "Pisa Mariners", "Modena Gialli", "Siena Stallions", "Trieste Port",
                    "Perugia Hill"
                ],
                PriorityTwoTeamNames:
                [
                    "Lecce Barocco", "Como Lago", "Taranto Ionio", "Mantova Virgil", "Padova Veneto",
                    "Vicenza Bianco", "Livorno Porto", "Ferrara Este", "Ancona Adriatico", "Cosenza Rossa",
                    "Foggia Tavoliere", "Pescara Delfini", "Arezzo Rosso", "Lucca Mura", "Novara Piemonte",
                    "Cremona Violini"
                ],
                FirstNames:
                [
                    "Luca", "Marco", "Giovanni", "Francesco", "Alessandro", "Matteo", "Andrea", "Giuseppe",
                    "Antonio", "Stefano", "Paolo", "Davide", "Simone", "Roberto", "Federico", "Lorenzo",
                    "Nicolo", "Salvatore", "Daniele", "Enrico"
                ],
                LastNames:
                [
                    "Rossi", "Russo", "Ferrari", "Esposito", "Bianchi", "Romano", "Colombo", "Ricci",
                    "Marino", "Greco", "Bruno", "Gallo", "Conti", "DeLuca", "Moretti", "Barbieri",
                    "Lombardi", "Fontana", "Caruso", "Vitale"
                ],
                Cities:
                [
                    "Rome", "Milan", "Naples", "Turin", "Florence",
                    "Genoa", "Bologna", "Verona", "Palermo", "Bari"
                ]),
            ["France"] = new(
                PriorityOneTeamNames:
                [
                    "Paris Lumiere", "Lyonnais FC", "Marseille Bleu", "Bordeaux Vignes", "Lille Nord",
                    "Nice Azur", "Nantes Loire", "Toulouse Garonne", "Monaco Rouge", "Rennes Armor",
                    "Strasbourg Etoile", "Montpellier Herault", "Grenoble Alpes", "Reims Champagne", "Saint Etienne Vert",
                    "Le Havre Ocean"
                ],
                PriorityTwoTeamNames:
                [
                    "Rouen Seine", "Tours Loire", "Amiens Picardie", "Brest Oceanique", "Angers Maine",
                    "Clermont Auvergne", "Nancy Lorraine", "Orleans Loiret", "Mulhouse Alsace", "Poitiers Vienne",
                    "Avignon Rhone", "Perpignan Catalan", "Limoges Porcelaine", "Annecy Lac", "Troyes Aube",
                    "Le Mans Sarthe"
                ],
                FirstNames:
                [
                    "Jean", "Pierre", "Michel", "Antoine", "Nicolas", "Julien", "Thomas", "Alexandre",
                    "Maxime", "Lucas", "Hugo", "Baptiste", "Adrien", "Arthur", "Mathis", "Quentin",
                    "Romain", "Florian", "Theo", "Victor"
                ],
                LastNames:
                [
                    "Martin", "Bernard", "Thomas", "Petit", "Robert", "Richard", "Durand", "Dubois",
                    "Moreau", "Laurent", "Simon", "Michel", "Lefevre", "Leroy", "Roux", "David",
                    "Bertrand", "Morel", "Fournier", "Girard"
                ],
                Cities:
                [
                    "Paris", "Lyon", "Marseille", "Bordeaux", "Lille",
                    "Nice", "Nantes", "Toulouse", "Strasbourg", "Montpellier"
                ]),
            ["Germany"] = new(
                PriorityOneTeamNames:
                [
                    "Berlin Adler", "Munich Isar", "Hamburg Harbor", "Cologne Dom", "Dortheim FC",
                    "Leipzig Roten", "Stuttgart Engine", "Bremen Weser", "Frankfurt Main", "Dresden Elbe",
                    "Hanover Horses", "Nuremberg Castle", "Essen Steel", "Kiel Baltic", "Freiburg Forest",
                    "Augsburg Gate"
                ],
                PriorityTwoTeamNames:
                [
                    "Bochum Ruhr", "Karlsruhe Baden", "Lubeck Hanse", "Regensburg Danube", "Aachen Gate",
                    "Magdeburg Elbe", "Kassel Hessen", "Ulm Spatzen", "Bielefeld Armin", "Saarbrucken Coal",
                    "Jena Optics", "Erfurt Garden", "Potsdam Crown", "Chemnitz Forge", "Mannheim Harbor",
                    "Osnabruck Bridge"
                ],
                FirstNames:
                [
                    "Lukas", "Leon", "Felix", "Jonas", "Paul", "Maximilian", "Tim", "Julian",
                    "Nico", "Tobias", "Florian", "Marcel", "Daniel", "Christian", "Patrick", "Alexander",
                    "Stefan", "Martin", "Kevin", "Johannes"
                ],
                LastNames:
                [
                    "Muller", "Schmidt", "Schneider", "Fischer", "Weber", "Meyer", "Wagner", "Becker",
                    "Hoffmann", "Schulz", "Koch", "Bauer", "Richter", "Klein", "Wolf", "Schroder",
                    "Neumann", "Schwarz", "Zimmermann", "Krause"
                ],
                Cities:
                [
                    "Berlin", "Munich", "Hamburg", "Cologne", "Frankfurt",
                    "Stuttgart", "Dortmund", "Leipzig", "Dresden", "Bremen"
                ]),
            ["Netherlands"] = new(
                PriorityOneTeamNames:
                [
                    "Amsterdam Canal", "Rotterdam Maas", "Eindhoven Light", "Utrecht Dom", "The Hague Coast",
                    "Groningen North", "Twente Enschede", "Arnhem Rhine", "Tilburg Textile", "Breda Nassau",
                    "Nijmegen Waal", "Haarlem Spaarne", "Leiden Keys", "Zwolle IJssel", "Alkmaar Cheese",
                    "Heerenveen Fries"
                ],
                PriorityTwoTeamNames:
                [
                    "Dordrecht Merwede", "Maastricht Maas", "Venlo Border", "Apeldoorn Veluwe", "Amersfoort Gate",
                    "Delft Blue", "Leeuwarden Crown", "Almere Flevo", "Deventer Eagles", "Helmond Steel",
                    "Oss Brabant", "Roosendaal Rose", "Sittard Fortune", "Volendam Harbor", "Emmen Drenthe",
                    "Den Bosch Dragons"
                ],
                FirstNames:
                [
                    "Daan", "Luuk", "Sem", "Bram", "Milan", "Finn", "Lars", "Jesse",
                    "Thijs", "Noah", "Tim", "Koen", "Niels", "Ruben", "Wout", "Joris",
                    "Sven", "Dirk", "Pieter", "Floris"
                ],
                LastNames:
                [
                    "De Jong", "Jansen", "De Vries", "Van den Berg", "Van Dijk", "Bakker", "Visser", "Smit",
                    "Meijer", "Boer", "Mulder", "De Groot", "Bos", "Vos", "Peters", "Hendriks",
                    "Van Leeuwen", "Dekker", "Brouwer", "Dijkstra"
                ],
                Cities:
                [
                    "Amsterdam", "Rotterdam", "Eindhoven", "Utrecht", "The Hague",
                    "Groningen", "Enschede", "Arnhem", "Tilburg", "Breda"
                ]),
            ["Spain"] = new(
                PriorityOneTeamNames:
                [
                    "Madrid Castile", "Barcelona Mar", "Valencia Turia", "Sevilla Giralda", "Bilbao Iron",
                    "Malaga Costa", "Zaragoza Ebro", "Granada Sierra", "Vigo Atlantico", "Alicante Sol",
                    "Murcia Huerta", "Valladolid Pucela", "Pamplona Navarre", "Santander Bahia", "Cordoba Califa",
                    "Gijon Norte"
                ],
                PriorityTwoTeamNames:
                [
                    "Cadiz Bahia", "Tarragona Tarraco", "Huelva Colombino", "Burgos Cid", "Logrono Rioja",
                    "Almeria Indalo", "Salamanca Tormes", "Oviedo Azul", "Cartagena Puerto", "Elche Palmeral",
                    "Badajoz Guadiana", "Leon Reino", "Jaen Oliva", "Lleida Segre", "Huesca Pirineos",
                    "Albacete Llanos"
                ],
                FirstNames:
                [
                    "Alejandro", "Daniel", "Pablo", "David", "Adrian", "Javier", "Sergio", "Carlos",
                    "Alvaro", "Diego", "Marcos", "Hugo", "Mario", "Raul", "Ivan", "Miguel",
                    "Fernando", "Ruben", "Jorge", "Andres"
                ],
                LastNames:
                [
                    "Garcia", "Rodriguez", "Gonzalez", "Fernandez", "Lopez", "Martinez", "Sanchez", "Perez",
                    "Gomez", "Martin", "Jimenez", "Ruiz", "Hernandez", "Diaz", "Moreno", "Munoz",
                    "Alvarez", "Romero", "Alonso", "Gutierrez"
                ],
                Cities:
                [
                    "Madrid", "Barcelona", "Valencia", "Seville", "Bilbao",
                    "Malaga", "Zaragoza", "Granada", "Vigo", "Alicante"
                ]),
            ["Portugal"] = new(
                PriorityOneTeamNames:
                [
                    "Lisboa Tejo", "Porto Douro", "Braga Minho", "Coimbra Mondego", "Faro Algarve",
                    "Guimaraes Castle", "Setubal Sado", "Aveiro Ria", "Funchal Madeira", "Leiria Pinhal",
                    "Viseu Durienses", "Evora Alentejo", "Santarem Ribatejo", "Vila Real Tras Montes", "Portimao Praia",
                    "Covilha Serra"
                ],
                PriorityTwoTeamNames:
                [
                    "Barreiro Industrial", "Matosinhos Atlantico", "Chaves Norte", "Beja Planicie", "Tomar Templarios",
                    "Lagos Costa", "Ponta Delgada Acores", "Braganca Nordeste", "Castelo Branco Raia", "Caldas Rainha",
                    "Amadora Estrela", "Oeiras Mar", "Viana Castelo Lima", "Torres Vedras Linhas", "Penafiel Vale",
                    "Almada Cristo"
                ],
                FirstNames:
                [
                    "Joao", "Miguel", "Diogo", "Tiago", "Pedro", "Andre", "Rafael", "Goncalo",
                    "Francisco", "Ricardo", "Bruno", "Luis", "Hugo", "Nuno", "Rui", "Carlos",
                    "Tomas", "Martim", "Duarte", "Samuel"
                ],
                LastNames:
                [
                    "Silva", "Santos", "Ferreira", "Pereira", "Oliveira", "Costa", "Rodrigues", "Martins",
                    "Jesus", "Sousa", "Fernandes", "Goncalves", "Gomes", "Lopes", "Marques", "Alves",
                    "Almeida", "Ribeiro", "Carvalho", "Teixeira"
                ],
                Cities:
                [
                    "Lisbon", "Porto", "Braga", "Coimbra", "Faro",
                    "Guimaraes", "Setubal", "Aveiro", "Funchal", "Leiria"
                ]),
            ["United States"] = new(
                PriorityOneTeamNames:
                [
                    "Liberty FC", "Pacific Sound", "Chicago Blaze", "Texas Lone Stars", "Boston Harbor",
                    "Atlanta Peaks", "Seattle Emerald", "Phoenix Heat", "Denver Summit", "Miami Atlantic",
                    "Detroit Motors", "Nashville Rhythm", "Portland Pines", "San Diego Surf", "Dallas Rangers",
                    "Houston Comets"
                ],
                PriorityTwoTeamNames:
                [
                    "Sacramento Gold", "Charlotte Crown", "Columbus Crewmen", "St Louis Gateway", "Cincinnati River",
                    "Tampa Bay Storm", "Baltimore Forge", "Austin Oaks", "Las Vegas Lights", "Kansas City Plains",
                    "Indianapolis Circle", "Pittsburgh Steel", "Raleigh Capital", "Milwaukee Lake", "Memphis Blues",
                    "New Orleans Jazz"
                ],
                FirstNames:
                [
                    "Michael", "Christopher", "Matthew", "Joshua", "Andrew", "David", "John", "Joseph",
                    "Anthony", "Nicholas", "Tyler", "Brandon", "Ryan", "Justin", "Kevin", "Jason",
                    "Zachary", "Christian", "Austin", "Logan"
                ],
                LastNames:
                [
                    "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
                    "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Moore",
                    "Jackson", "Martin", "Lee", "Perez"
                ],
                Cities:
                [
                    "New York", "Los Angeles", "Chicago", "Houston", "Phoenix",
                    "Philadelphia", "San Antonio", "San Diego", "Dallas", "Seattle"
                ]),
            ["Canada"] = new(
                PriorityOneTeamNames:
                [
                    "Toronto Maple FC", "Montreal Nord", "Vancouver Tides", "Ottawa Capital", "Calgary Peaks",
                    "Edmonton Aurora", "Winnipeg Plains", "Quebec Citadelle", "Halifax Harbor", "Saskatoon Prairie",
                    "Hamilton Steel", "Victoria Island", "London Ontario FC", "Regina Crown", "Kelowna Lake",
                    "Moncton Acadie"
                ],
                PriorityTwoTeamNames:
                [
                    "Trois Rivieres Bleu", "Laval Metro", "Brandon Wheat", "Red Deer Summit", "Whitehorse North",
                    "Yellowknife Aurora", "Prince George Timber", "Kamloops Heat", "Windsor Border", "Gatineau Hull",
                    "Sarnia Rapids", "Moose Jaw Prairie", "Thunder Bay Lake", "Charlottetown Crown", "Fredericton River",
                    "North Bay Trappers"
                ],
                FirstNames:
                [
                    "Liam", "Noah", "Ethan", "Lucas", "William", "Benjamin", "Logan", "Nathan",
                    "Samuel", "Jacob", "Thomas", "Charles", "Owen", "Jack", "Adam", "Julien",
                    "Antoine", "Gabriel", "Felix", "Connor"
                ],
                LastNames:
                [
                    "Smith", "Tremblay", "Gagnon", "Roy", "Cote", "Bouchard", "Martin", "Lefebvre",
                    "Wilson", "Johnson", "Campbell", "MacDonald", "Anderson", "Clark", "Reid", "Stewart",
                    "Fraser", "Murray", "Levesque", "Brown"
                ],
                Cities:
                [
                    "Toronto", "Montreal", "Vancouver", "Calgary", "Ottawa",
                    "Edmonton", "Winnipeg", "Quebec City", "Halifax", "Victoria"
                ]),
            ["Mexico"] = new(
                PriorityOneTeamNames:
                [
                    "Azteca Sur", "Monterrey Cerro", "Guadalajara Sol", "Puebla Blanca", "Tijuana Frontera",
                    "Veracruz Puerto", "Merida Maya", "Toluca Nevado", "Leon Bajio", "Queretaro Campanas",
                    "Oaxaca Monte", "Cancun Caribe", "Chiapas Selva", "Juarez Norte", "Culiacan Pacifico",
                    "Aguascalientes Ferro"
                ],
                PriorityTwoTeamNames:
                [
                    "Zacatecas Plata", "Tampico Marea", "Celaya Toros", "Irapuato Fresa", "Mazatlan Ola",
                    "Villahermosa Grijalva", "Colima Fuego", "Cuernavaca Primavera", "Tlaxcala Volcan", "Campeche Muralla",
                    "La Paz Baja", "Hermosillo Desierto", "Torreon Laguna", "Pachuca Mineros", "Ensenada Costa",
                    "Matamoros Frontera"
                ],
                FirstNames:
                [
                    "Jose", "Juan", "Luis", "Carlos", "Miguel", "Alejandro", "Francisco", "Diego",
                    "Ricardo", "Antonio", "Manuel", "Fernando", "Jesus", "Roberto", "Pedro", "Javier",
                    "Raul", "Hector", "Andres", "Emiliano"
                ],
                LastNames:
                [
                    "Hernandez", "Gonzalez", "Lopez", "Martinez", "Rodriguez", "Perez", "Sanchez", "Ramirez",
                    "Cruz", "Flores", "Gomez", "Diaz", "Reyes", "Morales", "Ortiz", "Castillo",
                    "Rojas", "Navarro", "Vargas", "Mendoza"
                ],
                Cities:
                [
                    "Mexico City", "Guadalajara", "Monterrey", "Puebla", "Tijuana",
                    "Merida", "Veracruz", "Leon", "Queretaro", "Juarez"
                ]),
            ["Argentina"] = new(
                PriorityOneTeamNames:
                [
                    "Buenos Aires Sur", "Cordoba Central", "Rosario Norte", "Mendoza Andes", "La Plata Estrella",
                    "Mar del Plata Port", "Tucuman Azucar", "Salta Valles", "Santa Fe Union", "Neuquen Patagonia",
                    "Bahia Blanca Wind", "San Juan Cuyo", "Corrientes River", "Posadas Misiones", "Parana Litoral",
                    "Jujuy Altura"
                ],
                PriorityTwoTeamNames:
                [
                    "Chaco Rojo", "Formosa Norte", "Trelew Patagonia", "Rawson Atlántico", "Junin Verde",
                    "Olavarria Cemento", "Rafaela Leche", "Moron Oeste", "Quilmes Cerveceros", "Tandil Sierras",
                    "San Rafael Sur", "Concordia Rio", "Gualeguay Blue", "Pergamino Fields", "Resistencia Litoral",
                    "Villa Maria Unido"
                ],
                FirstNames:
                [
                    "Juan", "Bautista", "Santiago", "Matias", "Nicolas", "Franco", "Agustin", "Tomas",
                    "Facundo", "Lautaro", "Luciano", "Martin", "Diego", "Federico", "Gonzalo", "Ramiro",
                    "Emiliano", "Ignacio", "Bruno", "Maximo"
                ],
                LastNames:
                [
                    "Gonzalez", "Rodriguez", "Gomez", "Fernandez", "Lopez", "Diaz", "Martinez", "Perez",
                    "Garcia", "Sanchez", "Romero", "Sosa", "Alvarez", "Torres", "Ruiz", "Suarez",
                    "Acosta", "Castro", "Medina", "Ortiz"
                ],
                Cities:
                [
                    "Buenos Aires", "Cordoba", "Rosario", "Mendoza", "La Plata",
                    "Mar del Plata", "Tucuman", "Salta", "Santa Fe", "Neuquen"
                ]),
            ["Brazil"] = new(
                PriorityOneTeamNames:
                [
                    "Rio Carioca", "Sao Paulo Paulista", "Belo Horizonte Mineiro", "Porto Alegre Sul", "Salvador Bahia",
                    "Recife Coral", "Fortaleza Sol", "Curitiba Pinheiros", "Manaus Amazonia", "Goiania Cerrado",
                    "Santos Praia", "Belem Norte", "Campinas Ponte", "Natal Dunas", "Florianopolis Ilha",
                    "Cuiaba Pantanal"
                ],
                PriorityTwoTeamNames:
                [
                    "Niteroi Guanabara", "Sao Luis Reggae", "Aracaju Sergipe", "Londrina Cafe", "Joinville Norte",
                    "Ribeirao Preto Interior", "Uberlandia Triangulo", "Pelotas Gaucho", "Juiz de Fora Zona", "Caxias Serra",
                    "Bauru Central", "Macapa Equator", "Boa Vista Branco", "Porto Velho Madeira", "Teresina Piaui",
                    "Petrolina Sao Francisco"
                ],
                FirstNames:
                [
                    "Joao", "Gabriel", "Pedro", "Lucas", "Matheus", "Rafael", "Bruno", "Felipe",
                    "Guilherme", "Thiago", "Andre", "Leonardo", "Caio", "Gustavo", "Daniel", "Henrique",
                    "Victor", "Diego", "Igor", "Vinicius"
                ],
                LastNames:
                [
                    "Silva", "Santos", "Oliveira", "Souza", "Rodrigues", "Ferreira", "Alves", "Pereira",
                    "Lima", "Gomes", "Ribeiro", "Carvalho", "Almeida", "Monteiro", "Melo", "Araujo",
                    "Costa", "Nascimento", "Barbosa", "Rocha"
                ],
                Cities:
                [
                    "Sao Paulo", "Rio de Janeiro", "Belo Horizonte", "Porto Alegre", "Salvador",
                    "Recife", "Fortaleza", "Curitiba", "Manaus", "Brasilia"
                ]),
            ["Japan"] = new(
                PriorityOneTeamNames:
                [
                    "Tokyo Sakura", "Osaka Kansai", "Yokohama Harbor", "Nagoya Shiro", "Sapporo Snow",
                    "Kyoto Kamo", "Kobe Port", "Fukuoka Hakata", "Hiroshima Peace", "Sendai Mori",
                    "Shizuoka Fuji", "Niigata Swan", "Kashima Antlers", "Nagasaki Bay", "Okayama Peach",
                    "Kumamoto Castle"
                ],
                PriorityTwoTeamNames:
                [
                    "Chiba Bay", "Saitama Reds", "Kanazawa Gold", "Matsumoto Alps", "Oita Onsen",
                    "Takamatsu Sanuki", "Mito Holly", "Utsunomiya Tochigi", "Toyama Tateyama", "Gifu Nagara",
                    "Yamagata Dewasanzan", "Akita Komachi", "Miyazaki Sun", "Kofu Kai", "Tokushima Awa",
                    "Tottori Dunes"
                ],
                FirstNames:
                [
                    "Haruto", "Yuto", "Sota", "Yuki", "Ren", "Kaito", "Daiki", "Takumi",
                    "Riku", "Shota", "Hayato", "Tsubasa", "Ryota", "Kenta", "Naoki", "Hiroki",
                    "Keisuke", "Makoto", "Kazuki", "Satoshi"
                ],
                LastNames:
                [
                    "Sato", "Suzuki", "Takahashi", "Tanaka", "Watanabe", "Ito", "Yamamoto", "Nakamura",
                    "Kobayashi", "Kato", "Yoshida", "Yamada", "Sasaki", "Yamaguchi", "Matsumoto", "Inoue",
                    "Kimura", "Hayashi", "Shimizu", "Mori"
                ],
                Cities:
                [
                    "Tokyo", "Osaka", "Yokohama", "Nagoya", "Sapporo",
                    "Kyoto", "Kobe", "Fukuoka", "Hiroshima", "Sendai"
                ]),
            ["Morocco"] = new(
                PriorityOneTeamNames:
                [
                    "Casablanca Atlas", "Rabat Oudayas", "Marrakesh Palm", "Tangier Strait", "Fes Medina",
                    "Agadir Souss", "Meknes Ismaili", "Oujda Orient", "Tetouan Rif", "Kenitra Sebou",
                    "Safi Atlantic", "El Jadida Coast", "Beni Mellal Tadla", "Nador Lagoon", "Khouribga Phosphate",
                    "Laayoune Sahara"
                ],
                PriorityTwoTeamNames:
                [
                    "Mohammedia Harbor", "Taza Mountain", "Settat Chaouia", "Khemisset Zemmour", "Errachidia Ziz",
                    "Taroudant Souss", "Guelmim Gate", "Larache Loukkos", "Essaouira Mogador", "Berkane Orange",
                    "Midelt High Atlas", "Tiznit Silver", "Azrou Cedar", "Dakhla Atlantic", "Taourirt Fortress",
                    "Sidi Kacem Gharb"
                ],
                FirstNames:
                [
                    "Youssef", "Mohamed", "Ahmed", "Hamza", "Omar", "Amine", "Anas", "Mehdi",
                    "Karim", "Ilyas", "Reda", "Zakaria", "Ayoub", "Bilal", "Nabil", "Samir",
                    "Hassan", "Sofiane", "Rachid", "Adil"
                ],
                LastNames:
                [
                    "El Amrani", "Benali", "Ait Lahcen", "El Idrissi", "Bennani", "Haddad", "Bouzid", "Cherkaoui",
                    "Fassi", "Tahiri", "Mansouri", "Alaoui", "El Mansouri", "Sbai", "Jaafari", "Berrada",
                    "Lamrani", "Naciri", "Kabbaj", "Essaid"
                ],
                Cities:
                [
                    "Casablanca", "Rabat", "Marrakesh", "Tangier", "Fes",
                    "Agadir", "Meknes", "Oujda", "Tetouan", "Kenitra"
                ]),
            ["Australia"] = new(
                PriorityOneTeamNames:
                [
                    "Sydney Harbour", "Melbourne Yarra", "Brisbane River", "Perth Swan", "Adelaide Torrens",
                    "Canberra Capital", "Newcastle Steel", "Gold Coast Surf", "Wollongong Illawarra", "Hobart Derwent",
                    "Darwin Top End", "Townsville Reef", "Geelong Bay", "Cairns Tropics", "Ballarat Gold",
                    "Launceston Tamar"
                ],
                PriorityTwoTeamNames:
                [
                    "Parramatta West", "Fremantle Dockers", "Toowoomba Range", "Bendigo Miners", "Mackay Coral",
                    "Rockhampton Bulls", "Albury Murray", "Wagga Plains", "Bunbury Coast", "Shepparton Goulburn",
                    "Mandurah Estuary", "Maitland Hunter", "Bathurst Mount", "Ipswich Valley", "Orange Central",
                    "Whyalla Spencer"
                ],
                FirstNames:
                [
                    "Jack", "Oliver", "William", "Noah", "Thomas", "Lucas", "Henry", "Lachlan",
                    "James", "Ethan", "Cooper", "Mason", "Charlie", "Harrison", "Max", "Samuel",
                    "Joshua", "Benjamin", "Archie", "Oscar"
                ],
                LastNames:
                [
                    "Smith", "Jones", "Williams", "Brown", "Wilson", "Taylor", "Johnson", "White",
                    "Martin", "Anderson", "Thompson", "Nguyen", "Walker", "Ryan", "Robinson", "Kelly",
                    "King", "Campbell", "Wright", "Mitchell"
                ],
                Cities:
                [
                    "Sydney", "Melbourne", "Brisbane", "Perth", "Adelaide",
                    "Canberra", "Newcastle", "Gold Coast", "Wollongong", "Hobart"
                ])
        };

        private sealed record NationGenerationData(
            string[] PriorityOneTeamNames,
            string[] PriorityTwoTeamNames,
            string[] FirstNames,
            string[] LastNames,
            string[] Cities)
        {
            public string[] GetTeamNamesForPriority(int priority) => priority == 2 ? PriorityTwoTeamNames : PriorityOneTeamNames;
        }

        private readonly record struct FormationSlot(PlayerPosition Position, PlayerRole Role);

        private static readonly Dictionary<Formation, FormationSlot[]> FormationSlotsByFormation = new()
        {
            [Formation.Four_Four_Two] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.LeftMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.RightMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.RightStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Four_Three_Three] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.CentralCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.RightStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.CentralStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Three_Five_Two] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.CentralCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.CentralCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.RightStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Five_Three_Two] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.CentralCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.CentralCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.RightStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Four_Five_One] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.LeftMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.CentralCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.RightMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.CentralStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Four_Two_Three_One] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.RightDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.LeftDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.RightAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.CentralAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.LeftAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.CentralStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Four_Three_Two_One] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.CentralCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.RightAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.LeftAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.CentralStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Four_One_Four_One] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.CentralDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.LeftMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.RightMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.CentralStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Four_Four_One_One] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.LeftMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.RightMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.CentralAttackingMidfielder, PlayerRole.SecondStriker),
                new(PlayerPosition.CentralStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Four_Two_Two_Two] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.RightDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.LeftDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.RightAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.LeftAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.RightStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Four_Four_Two_Diamond] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.CentralDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.LeftMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.RightMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.CentralAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.RightStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Four_Three_One_Two] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.CentralCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.CentralAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.RightStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Four_One_Three_Two] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.CentralDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.LeftMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.CentralCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.RightMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.RightStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Four_One_Two_One_Two] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.CentralDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.CentralAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.RightStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Three_Four_Three] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.CentralCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightWingBack, PlayerRole.WingBack),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftWingBack, PlayerRole.WingBack),
                new(PlayerPosition.RightWinger, PlayerRole.Winger),
                new(PlayerPosition.CentralStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftWinger, PlayerRole.Winger)
            ],
            [Formation.Three_Four_Two_One] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.CentralCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightWingBack, PlayerRole.WingBack),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftWingBack, PlayerRole.WingBack),
                new(PlayerPosition.RightAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.LeftAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.CentralStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Three_Four_One_Two] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.CentralCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightWingBack, PlayerRole.WingBack),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftWingBack, PlayerRole.WingBack),
                new(PlayerPosition.CentralAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.RightStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Three_Three_Four] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.CentralCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.CentralDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.LeftDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.RightWinger, PlayerRole.Winger),
                new(PlayerPosition.RightStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftWinger, PlayerRole.Winger)
            ],
            [Formation.Three_Six_One] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.CentralCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.CentralCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.CentralAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.CentralStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Three_Three_Two_Two] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.CentralCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.CentralDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.LeftDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.RightAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.LeftAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.RightStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Three_Two_Three_Two] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.CentralCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.LeftDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.RightAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.CentralAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.LeftAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.RightStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Five_Four_One] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.CentralCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftMidfielder, PlayerRole.WideMidfielder),
                new(PlayerPosition.CentralStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Five_Two_Three] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.RightWingBack, PlayerRole.WingBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.CentralCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftWingBack, PlayerRole.WingBack),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.RightWinger, PlayerRole.Winger),
                new(PlayerPosition.CentralStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftWinger, PlayerRole.Winger)
            ],
            [Formation.Five_Three_One_One] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.RightWingBack, PlayerRole.WingBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.CentralCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftWingBack, PlayerRole.WingBack),
                new(PlayerPosition.RightCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.CentralCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.LeftCenterMidfielder, PlayerRole.CentralMidfielder),
                new(PlayerPosition.CentralAttackingMidfielder, PlayerRole.SecondStriker),
                new(PlayerPosition.CentralStriker, PlayerRole.AdvancedForward)
            ],
            [Formation.Four_Six_Zero] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.LeftBack, PlayerRole.FullBack),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightBack, PlayerRole.FullBack),
                new(PlayerPosition.RightDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.LeftDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.RightWinger, PlayerRole.Winger),
                new(PlayerPosition.RightAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.LeftAttackingMidfielder, PlayerRole.AttackingMidfielder),
                new(PlayerPosition.LeftWinger, PlayerRole.Winger)
            ],
            [Formation.Two_Three_Five] =
            [
                new(PlayerPosition.Goalkeeper, PlayerRole.Goalkeeper),
                new(PlayerPosition.RightCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.LeftCenterBack, PlayerRole.CenterBack),
                new(PlayerPosition.RightDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.CentralDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.LeftDefensiveMidfielder, PlayerRole.DefensiveMidfielder),
                new(PlayerPosition.RightWinger, PlayerRole.Winger),
                new(PlayerPosition.RightStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.CentralStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftStriker, PlayerRole.AdvancedForward),
                new(PlayerPosition.LeftWinger, PlayerRole.Winger)
            ]
        };

        private static readonly Dictionary<PlayerPosition, PlayerPosition[]> _positionTripletes = new()
        {
            // Center-Back triplete
            { PlayerPosition.RightCenterBack,            new[] { PlayerPosition.CentralCenterBack,        PlayerPosition.LeftCenterBack } },
            { PlayerPosition.CentralCenterBack,          new[] { PlayerPosition.RightCenterBack,          PlayerPosition.LeftCenterBack } },
            { PlayerPosition.LeftCenterBack,             new[] { PlayerPosition.RightCenterBack,          PlayerPosition.CentralCenterBack } },
            // Defensive-Midfielder triplete
            { PlayerPosition.RightDefensiveMidfielder,   new[] { PlayerPosition.CentralDefensiveMidfielder, PlayerPosition.LeftDefensiveMidfielder } },
            { PlayerPosition.CentralDefensiveMidfielder, new[] { PlayerPosition.RightDefensiveMidfielder,   PlayerPosition.LeftDefensiveMidfielder } },
            { PlayerPosition.LeftDefensiveMidfielder,    new[] { PlayerPosition.RightDefensiveMidfielder,   PlayerPosition.CentralDefensiveMidfielder } },
            // Center-Midfielder triplete
            { PlayerPosition.RightCenterMidfielder,      new[] { PlayerPosition.CentralCenterMidfielder,  PlayerPosition.LeftCenterMidfielder } },
            { PlayerPosition.CentralCenterMidfielder,    new[] { PlayerPosition.RightCenterMidfielder,    PlayerPosition.LeftCenterMidfielder } },
            { PlayerPosition.LeftCenterMidfielder,       new[] { PlayerPosition.RightCenterMidfielder,    PlayerPosition.CentralCenterMidfielder } },
            // Attacking-Midfielder triplete
            { PlayerPosition.RightAttackingMidfielder,   new[] { PlayerPosition.CentralAttackingMidfielder, PlayerPosition.LeftAttackingMidfielder } },
            { PlayerPosition.CentralAttackingMidfielder, new[] { PlayerPosition.RightAttackingMidfielder,   PlayerPosition.LeftAttackingMidfielder } },
            { PlayerPosition.LeftAttackingMidfielder,    new[] { PlayerPosition.RightAttackingMidfielder,   PlayerPosition.CentralAttackingMidfielder } },
            // Striker triplete
            { PlayerPosition.RightStriker,               new[] { PlayerPosition.CentralStriker,           PlayerPosition.LeftStriker } },
            { PlayerPosition.CentralStriker,             new[] { PlayerPosition.RightStriker,             PlayerPosition.LeftStriker } },
            { PlayerPosition.LeftStriker,                new[] { PlayerPosition.RightStriker,             PlayerPosition.CentralStriker } },
        };

        private readonly SoccerDbContext _context;

        public TeamGenerationService(SoccerDbContext context)
        {
            _context = context;
        }

        public async Task<List<Team>> GenerateTeamsForCompetition(Guid? serverID, Guid? nationID, int numberOfTeams = 16, int priority = 1)
        {
            var teams = new List<Team>();
            var random = new Random();

            var nations = await _context.Nations.ToListAsync();
            var nationsById = nations.ToDictionary(n => n.NationID);
            var targetNation = nations.FirstOrDefault(n => n.NationID == nationID);
            var teamGenerationData = GetGenerationData(targetNation);
            var restNations = nations.Where(n => n.NationID != nationID).ToList();
            var kitShapes = Enum.GetValues<KitShapeEnum>();

            // Create a shuffled copy of team names to avoid duplicates
            var availableNames = teamGenerationData.GetTeamNamesForPriority(priority).ToList();
            
            for (int i = 0; i < numberOfTeams && availableNames.Count > 0; i++)
            {
                // Pick a random name from available names
                var randomIndex = random.Next(availableNames.Count);
                var teamName = availableNames[randomIndex];
                
                // Remove the name from available names to prevent duplicates
                availableNames.RemoveAt(randomIndex);

                Stadium stadium = new Stadium
                {
                    StadiumID = Guid.NewGuid(),
                    Name = $"{teamName} Stadium",
                    Capacity = random.Next(20000, 80001), // Random capacity between 20,000 and 80,000
                    Latitude = random.NextDouble() * 180 - 90, // Random latitude between -90 and 90
                    Longitude = random.NextDouble() * 360 - 180, // Random longitude between -180 and 180
                    City = teamGenerationData.Cities[random.Next(teamGenerationData.Cities.Length)]
                };

                _context.Add(stadium);

                Kit kit = new Kit
                {
                    KitID = Guid.NewGuid(),
                    HomeShirtColor = $"#{random.Next(0x1000000):X6}",
                    HomeShortsColor = $"#{random.Next(0x1000000):X6}",
                    AwayShirtColor = $"#{random.Next(0x1000000):X6}",
                    AwayShortsColor = $"#{random.Next(0x1000000):X6}",
                    KitShape = kitShapes[random.Next(kitShapes.Length)]
                };

                _context.Kits.Add(kit);

                var team = new Team
                {
                    TeamID = Guid.NewGuid(),
                    Name = teamName,
                    Competitions = new List<Competition>(),
                    Contracts = new List<Contract>(),
                    Code = BuildTeamCode(teamName),
                    StadiumID = stadium.StadiumID,
                    KitID = kit.KitID
                };

                // Generate primary tactic for the team
                var primaryTactic = new Tactic
                {
                    TacticID = Guid.NewGuid(),
                    TeamID = team.TeamID,
                    Name = "Primary Tactic",
                    Formation = Formation.Four_Four_Two,
                    isMain = true
                };
                _context.Tactics.Add(primaryTactic);

                // Store player IDs for later position assignment
                var teamPlayerIDs = new List<Guid>();
                var teamPlayerStats = new List<PlayerStats>();

                List<byte> teamShirtNumbersAssigned = new List<byte>();
                var requiredPrimaryPositions = BuildRequiredPrimaryPlayerPositions(random);

                // Generate 30 people for each team
                for (int j = 0; j < 30; j++)
                {
                    var personID = Guid.NewGuid();
                    var assignedNationID = nationID;
                    if (restNations.Count > 0 && random.Next(0, 10) >= 7)
                    {
                        assignedNationID = restNations[random.Next(restNations.Count)].NationID;
                    }

                    Nation? assignedNation = null;
                    if (assignedNationID.HasValue)
                    {
                        nationsById.TryGetValue(assignedNationID.Value, out assignedNation);
                    }
                    var personGenerationData = GetGenerationData(assignedNation ?? targetNation);

                    // Create Person
                    int randomWeight = Random.Shared.Next(75, 95);
                    int randomHeight = Random.Shared.Next(175, 195);

                    var person = new Person
                    {
                        PersonID = personID,
                        Name = personGenerationData.FirstNames[random.Next(personGenerationData.FirstNames.Length)],
                        Surname = personGenerationData.LastNames[random.Next(personGenerationData.LastNames.Length)],
                        DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-random.Next(18, 35)).AddDays(-random.Next(0, 365))),
                        PlaceOfBirth = personGenerationData.Cities[random.Next(personGenerationData.Cities.Length)],
                        NationID = assignedNationID,
                        ServerID = serverID,
                        PlayerTrainedPositions = new List<PlayerTrainedPosition>(),
                        PlayerTrainedRoles = new List<PlayerTrainedRole>(),
                        Weight = randomWeight,
                        Height = randomHeight
                    };

                    var personHealthAndFitness = new PersonHealthAndFitness
                    {
                        PersonID = personID,
                        HealthStatus = HealthStatus.Healthy,
                        PhysicalCondition = (byte)random.Next(10, 100),
                        MentalCondition = (byte)random.Next(10, 100),
                        FitnessCondition = (byte)random.Next(10, 100)
                    };

                    // random Wage per week, from 500 to 5000
                    var randomWagePerWeek = random.Next(5, 50);
                    randomWagePerWeek *= 100;

                    // Note: Changed first date to be one day before and last day at least in 2027 for debugging reasons
                    // Create Contract with random end date (June 30, random year 2027-2030)
                    var contractEndYear = random.Next(2027, 2031); // 2031 is exclusive, so 2027-2030
                    var contract = new Contract
                    {
                        ContractID = Guid.NewGuid(),
                        PersonID = personID,
                        TeamID = team.TeamID,
                        StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1),
                        EndDate = new DateOnly(contractEndYear, 6, 30),
                        Role = Role.Player,
                        Wage = randomWagePerWeek
                    };

                    // Generate PlayerTrainedPositions
                    var requiredPrimaryPosition = j < requiredPrimaryPositions.Count ? requiredPrimaryPositions[j] : (PlayerPosition?)null;
                    var trainedPositions = GeneratePlayerTrainedPositions(random, personID, requiredPrimaryPosition);
                    
                    // Generate PlayerTrainedRoles for each PlayerTrainedPosition
                    var trainedRoles = new List<PlayerTrainedRole>();
                    foreach (var trainedPosition in trainedPositions)
                    {
                        var rolesForPosition = GeneratePlayerTrainedRoles(random, personID, trainedPosition.PlayerPosition);
                        trainedRoles.AddRange(rolesForPosition);
                    }

                    byte shirtNumber = (byte)random.Next(1, 100);

                    if(teamShirtNumbersAssigned.Contains(shirtNumber))
                    {
                        // If shirt number is already assigned, find the next available one
                        shirtNumber = 1;
                        while (teamShirtNumbersAssigned.Contains(shirtNumber) && shirtNumber < 99)
                        {
                            shirtNumber++;
                        }
                    }

                    teamShirtNumbersAssigned.Add(shirtNumber);
                    contract.ShirtNumber = shirtNumber;

                    PlayerStats playerStats = AssignPlayerStatsToNewPlayer(personID);

                    // Add entities to database context
                    _context.People.Add(person);
                    _context.PersonHealthAndFitnesses.Add(personHealthAndFitness);
                    _context.Contracts.Add(contract);
                    _context.PlayerStats.Add(playerStats);
                    _context.PlayerTrainedPositions.AddRange(trainedPositions);
                    _context.PlayerTrainedRoles.AddRange(trainedRoles);

                    // Store player ID for position assignment
                    teamPlayerIDs.Add(personID);
                    teamPlayerStats.Add(playerStats);
                }

                AssignBestTeamTacticPriorities(team.TeamID, teamPlayerStats);
                AddManagerToTeam(team, serverID, nationID, targetNation, teamGenerationData, nationsById, random);
                AddCoachesToTeam(team, serverID, nationID, targetNation, teamGenerationData, nationsById, random);
                AddMedicsToTeam(team, serverID, nationID, targetNation, teamGenerationData, nationsById, random);

                teams.Add(team);
            }

            return teams;
        }

        private void AddManagerToTeam(
            Team team,
            Guid? serverID,
            Guid? nationID,
            Nation? targetNation,
            NationGenerationData teamGenerationData,
            IReadOnlyDictionary<Guid, Nation> nationsById,
            Random random)
        {
            var personID = Guid.NewGuid();
            var personGenerationData = teamGenerationData;

            if (nationID.HasValue && nationsById.TryGetValue(nationID.Value, out var managerNation))
            {
                personGenerationData = GetGenerationData(managerNation);
            }
            else if (targetNation != null)
            {
                personGenerationData = GetGenerationData(targetNation);
            }

            var person = new Person
            {
                PersonID = personID,
                Name = personGenerationData.FirstNames[random.Next(personGenerationData.FirstNames.Length)],
                Surname = personGenerationData.LastNames[random.Next(personGenerationData.LastNames.Length)],
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-random.Next(35, 66)).AddDays(-random.Next(0, 365))),
                PlaceOfBirth = personGenerationData.Cities[random.Next(personGenerationData.Cities.Length)],
                NationID = nationID,
                ServerID = serverID,
                StaffRole = StaffRole.Manager,
                Weight = random.Next(70, 101),
                Height = random.Next(165, 196)
            };

            var personHealthAndFitness = new PersonHealthAndFitness
            {
                PersonHealthAndFitnessID = Guid.NewGuid(),
                PersonID = personID,
                HealthStatus = HealthStatus.Healthy,
                PhysicalCondition = (byte)random.Next(70, 101),
                MentalCondition = (byte)random.Next(70, 101),
                FitnessCondition = (byte)random.Next(70, 101)
            };

            var contract = new Contract
            {
                ContractID = Guid.NewGuid(),
                PersonID = personID,
                TeamID = team.TeamID,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1),
                EndDate = null,
                Role = Role.Staff,
                Wage = random.Next(10, 100) * 100
            };

            _context.People.Add(person);
            _context.PersonHealthAndFitnesses.Add(personHealthAndFitness);
            _context.Contracts.Add(contract);
        }

        private void AddCoachesToTeam(
            Team team,
            Guid? serverID,
            Guid? nationID,
            Nation? targetNation,
            NationGenerationData teamGenerationData,
            IReadOnlyDictionary<Guid, Nation> nationsById,
            Random random)
        {
            var personGenerationData = teamGenerationData;

            if (nationID.HasValue && nationsById.TryGetValue(nationID.Value, out var coachNation))
            {
                personGenerationData = GetGenerationData(coachNation);
            }
            else if (targetNation != null)
            {
                personGenerationData = GetGenerationData(targetNation);
            }

            for (var i = 0; i < 2; i++)
            {
                var person = CreateStaffPerson(
                    StaffRole.Coach,
                    serverID,
                    nationID,
                    personGenerationData,
                    random,
                    25,
                    56);

                var coachStats = new CoachStats
                {
                    CoachStatsID = Guid.NewGuid(),
                    PersonID = person.PersonID,
                    Attack = (byte)random.Next(1, 101),
                    Defend = (byte)random.Next(1, 101),
                    Control = (byte)random.Next(1, 101),
                    Goalkeeper = (byte)random.Next(1, 101),
                    Tactic = (byte)random.Next(1, 101),
                    Fitness = (byte)random.Next(1, 101)
                };

                var contract = CreateStaffContract(person.PersonID, team.TeamID, random.Next(5, 50) * 100);

                _context.People.Add(person);
                _context.CoachStats.Add(coachStats);
                _context.Contracts.Add(contract);
            }
        }

        private void AddMedicsToTeam(
            Team team,
            Guid? serverID,
            Guid? nationID,
            Nation? targetNation,
            NationGenerationData teamGenerationData,
            IReadOnlyDictionary<Guid, Nation> nationsById,
            Random random)
        {
            var personGenerationData = teamGenerationData;

            if (nationID.HasValue && nationsById.TryGetValue(nationID.Value, out var medicNation))
            {
                personGenerationData = GetGenerationData(medicNation);
            }
            else if (targetNation != null)
            {
                personGenerationData = GetGenerationData(targetNation);
            }

            for (var i = 0; i < 2; i++)
            {
                var person = CreateStaffPerson(
                    StaffRole.Medic,
                    serverID,
                    nationID,
                    personGenerationData,
                    random,
                    25,
                    56);

                var medicStats = new MedicStats
                {
                    MedicStatsID = Guid.NewGuid(),
                    PersonID = person.PersonID,
                    Diagnosis = (byte)random.Next(1, 101),
                    Treatment = (byte)random.Next(1, 101),
                    Rehabilitation = (byte)random.Next(1, 101),
                    Prevention = (byte)random.Next(1, 101)
                };

                var contract = CreateStaffContract(person.PersonID, team.TeamID, random.Next(5, 50) * 100);

                _context.People.Add(person);
                _context.MedicStats.Add(medicStats);
                _context.Contracts.Add(contract);
            }
        }

        private static Person CreateStaffPerson(
            StaffRole staffRole,
            Guid? serverID,
            Guid? nationID,
            NationGenerationData personGenerationData,
            Random random,
            int minimumAge,
            int maximumAgeExclusive)
        {
            return new Person
            {
                PersonID = Guid.NewGuid(),
                Name = personGenerationData.FirstNames[random.Next(personGenerationData.FirstNames.Length)],
                Surname = personGenerationData.LastNames[random.Next(personGenerationData.LastNames.Length)],
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-random.Next(minimumAge, maximumAgeExclusive)).AddDays(-random.Next(0, 365))),
                PlaceOfBirth = personGenerationData.Cities[random.Next(personGenerationData.Cities.Length)],
                NationID = nationID,
                ServerID = serverID,
                StaffRole = staffRole,
                Weight = random.Next(70, 101),
                Height = random.Next(165, 196)
            };
        }

        private static Contract CreateStaffContract(Guid personID, Guid teamID, int wage)
        {
            return new Contract
            {
                ContractID = Guid.NewGuid(),
                PersonID = personID,
                TeamID = teamID,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1),
                EndDate = null,
                Role = Role.Staff,
                Wage = wage
            };
        }

        private PlayerStats AssignPlayerStatsToNewPlayer(Guid PlayerID)
        {
            var random = new Random();

            // Create PlayerStats with random values between 1 and 100
            var playerStats = new PlayerStats
            {
                PlayerStatsID = Guid.NewGuid(),
                PersonID = PlayerID,
                Shooting = (byte)random.Next(1, 101),
                Passing = (byte)random.Next(1, 101),
                Crossing = (byte)random.Next(1, 101),
                Tackling = (byte)random.Next(1, 101),
                Dribbling = (byte)random.Next(1, 101),
                Control = (byte)random.Next(1, 101),
                Kicking = (byte)random.Next(1, 101),
                Goalkeeping = (byte)random.Next(1, 101),
                Teamwork = (byte)random.Next(1, 101),
                Creativity = (byte)random.Next(1, 101),
                Decisions = (byte)random.Next(1, 101),
                Positioning = (byte)random.Next(1, 101),
                Speed = (byte)random.Next(1, 101),
                Acceleration = (byte)random.Next(1, 101),
                Strength = (byte)random.Next(1, 101),
                Jumping = (byte)random.Next(1, 101),
                Stamina = (byte)random.Next(1, 101)
            };

            return playerStats;
        }

        private void AssignBestTeamTacticPriorities(Guid teamID, IReadOnlyCollection<PlayerStats> teamPlayerStats)
        {
            AddPrimaryTeamTacticPriority(teamID, TeamTacticPriorityType.Captain, GetBestCaptainID(teamPlayerStats));
            AddPrimaryTeamTacticPriority(teamID, TeamTacticPriorityType.Penalty, GetBestPenaltyTakerID(teamPlayerStats));

            Guid? bestCornerTakerID = GetBestCornerTakerID(teamPlayerStats);
            AddPrimaryTeamTacticPriority(teamID, TeamTacticPriorityType.LeftCornerKick, bestCornerTakerID);
            AddPrimaryTeamTacticPriority(teamID, TeamTacticPriorityType.RightCornerKick, bestCornerTakerID);

            Guid? bestFreeKickTakerID = GetBestFreeKickTakerID(teamPlayerStats);
            AddPrimaryTeamTacticPriority(teamID, TeamTacticPriorityType.LeftFreeKick, bestFreeKickTakerID);
            AddPrimaryTeamTacticPriority(teamID, TeamTacticPriorityType.RightFreeKick, bestFreeKickTakerID);
        }

        private void AddPrimaryTeamTacticPriority(Guid teamID, TeamTacticPriorityType type, Guid? personID)
        {
            if (!personID.HasValue)
            {
                return;
            }

            _context.TeamTacticPriorities.Add(new TeamTacticPriority
            {
                TeamTacticPriorityID = Guid.NewGuid(),
                TeamID = teamID,
                Type = type,
                PersonID = personID.Value,
                Priority = 1
            });
        }

        private static Guid? GetBestCaptainID(IReadOnlyCollection<PlayerStats> teamPlayerStats)
        {
            return GetBestPlayerID(teamPlayerStats, stats => (stats.Decisions + stats.Teamwork) / 2.0);
        }

        private static Guid? GetBestPenaltyTakerID(IReadOnlyCollection<PlayerStats> teamPlayerStats)
        {
            return GetBestPlayerID(teamPlayerStats, stats =>
                (stats.Shooting + stats.Kicking + stats.Decisions + stats.Strength) / 4.0);
        }

        private static Guid? GetBestCornerTakerID(IReadOnlyCollection<PlayerStats> teamPlayerStats)
        {
            return GetBestPlayerID(teamPlayerStats, stats =>
                (stats.Crossing + stats.Kicking + stats.Teamwork + stats.Decisions + stats.Strength) / 5.0);
        }

        private static Guid? GetBestFreeKickTakerID(IReadOnlyCollection<PlayerStats> teamPlayerStats)
        {
            return GetBestPlayerID(teamPlayerStats, stats =>
                (stats.Shooting + stats.Crossing + stats.Kicking + stats.Teamwork + stats.Decisions + stats.Strength) / 6.0);
        }

        private static Guid? GetBestPlayerID(IReadOnlyCollection<PlayerStats> teamPlayerStats, Func<PlayerStats, double> scoreSelector)
        {
            return teamPlayerStats
                .OrderByDescending(scoreSelector)
                .Select(stats => (Guid?)stats.PersonID)
                .FirstOrDefault();
        }

        private static NationGenerationData GetGenerationData(Nation? nation)
        {
            if (nation != null && GenerationDataByNation.TryGetValue(nation.Name, out var nationGenerationData))
            {
                return nationGenerationData;
            }

            if (GenerationDataByNation.Values.FirstOrDefault() is { } fallbackGenerationData)
            {
                return fallbackGenerationData;
            }

            throw new InvalidOperationException("No nation generation data has been configured.");
        }

        private static string BuildTeamCode(string teamName)
        {
            var sanitizedName = new string(teamName
                .Where(char.IsLetterOrDigit)
                .Select(char.ToUpperInvariant)
                .ToArray());

            if (string.IsNullOrWhiteSpace(sanitizedName))
            {
                var fallbackSeed = teamName.Aggregate(17L, (current, character) => (current * 31 + character) % 1000);
                return $"T{fallbackSeed:D3}";
            }

            return sanitizedName.Length >= 4
                ? sanitizedName[..4]
                : sanitizedName.PadRight(4, 'X');
        }

        private static List<PlayerPosition> BuildRequiredPrimaryPlayerPositions(Random random)
        {
            var positions = new List<PlayerPosition>();

            AddRepeatedPositions(positions, PlayerPosition.Goalkeeper, 2);
            AddRepeatedPositions(positions, PlayerPosition.RightBack, 2);
            AddRandomPositions(positions, random, 3, PlayerPosition.LeftCenterBack, PlayerPosition.CentralCenterBack, PlayerPosition.RightCenterBack);
            AddRepeatedPositions(positions, PlayerPosition.LeftBack, 2);
            AddRandomPositions(positions, random, 3, PlayerPosition.LeftCenterMidfielder, PlayerPosition.CentralCenterMidfielder, PlayerPosition.RightCenterMidfielder);
            AddRepeatedPositions(positions, PlayerPosition.LeftMidfielder, 2);
            AddRepeatedPositions(positions, PlayerPosition.RightMidfielder, 2);
            AddRandomPositions(positions, random, 3, PlayerPosition.LeftStriker, PlayerPosition.CentralStriker, PlayerPosition.RightStriker);

            return positions;
        }

        private static void AddRepeatedPositions(List<PlayerPosition> positions, PlayerPosition position, int count)
        {
            for (int i = 0; i < count; i++)
            {
                positions.Add(position);
            }
        }

        private static void AddRandomPositions(List<PlayerPosition> positions, Random random, int count, params PlayerPosition[] candidates)
        {
            for (int i = 0; i < count; i++)
            {
                positions.Add(candidates[random.Next(candidates.Length)]);
            }
        }

        private List<PlayerTrainedPosition> GeneratePlayerTrainedPositions(Random random, Guid personID, PlayerPosition? primaryPosition = null)
        {
            var trainedPositions = new List<PlayerTrainedPosition>();
            
            // Get all valid player positions (exclude None)
            var validPositions = Enum.GetValues(typeof(PlayerPosition))
                .Cast<PlayerPosition>()
                .Where(p => p != PlayerPosition.None)
                .ToList();

            // First trained position (80-100 adaptaption)
            var firstPosition = primaryPosition ?? validPositions[random.Next(validPositions.Count)];
            var firstAdaptation = (byte)random.Next(80, 101); // 80-100 inclusive
            trainedPositions.Add(new PlayerTrainedPosition
            {
                PlayerTrainedPositionID = Guid.NewGuid(),
                PersonID = personID,
                PlayerPosition = firstPosition,
                PlayerTrainedPositionAdaptation = firstAdaptation
            });

            // If firstPosition belongs to a triplete, also add the other 2 companion positions
            if (_positionTripletes.TryGetValue(firstPosition, out var firstCompanions))
            {
                foreach (var companion in firstCompanions)
                {
                    trainedPositions.Add(new PlayerTrainedPosition
                    {
                        PlayerTrainedPositionID = Guid.NewGuid(),
                        PersonID = personID,
                        PlayerPosition = companion,
                        PlayerTrainedPositionAdaptation = firstAdaptation
                    });
                }
            }

            // 15% chance for second trained position (50-80 adaptaption)
            if (random.Next(100) < 15)
            {
                // Make sure second position is different from all already-added positions
                var addedPositions = trainedPositions.Select(tp => tp.PlayerPosition).ToHashSet();
                var availablePositions = validPositions.Where(p => !addedPositions.Contains(p)).ToList();
                if (availablePositions.Any())
                {
                    var secondPosition = availablePositions[random.Next(availablePositions.Count)];
                    var secondAdaptation = (byte)random.Next(50, 81); // 50-80 inclusive
                    trainedPositions.Add(new PlayerTrainedPosition
                    {
                        PlayerTrainedPositionID = Guid.NewGuid(),
                        PersonID = personID,
                        PlayerPosition = secondPosition,
                        PlayerTrainedPositionAdaptation = secondAdaptation
                    });

                    // If secondPosition belongs to a triplete, also add the other 2 companion positions
                    if (_positionTripletes.TryGetValue(secondPosition, out var secondCompanions))
                    {
                        foreach (var companion in secondCompanions)
                        {
                            if (!addedPositions.Contains(companion))
                            {
                                trainedPositions.Add(new PlayerTrainedPosition
                                {
                                    PlayerTrainedPositionID = Guid.NewGuid(),
                                    PersonID = personID,
                                    PlayerPosition = companion,
                                    PlayerTrainedPositionAdaptation = secondAdaptation
                                });
                            }
                        }
                    }
                }
            }

            return trainedPositions;
        }

        private List<PlayerTrainedRole> GeneratePlayerTrainedRoles(Random random, Guid personID, PlayerPosition position)
        {
            var trainedRoles = new List<PlayerTrainedRole>();
            
            var validRoles = GetValidRolesForPosition(position);

            if (!validRoles.Any())
            {
                return trainedRoles;
            }

            // First trained role (80-100 adaptaption)
            var firstRole = validRoles[random.Next(validRoles.Count)];
            trainedRoles.Add(new PlayerTrainedRole
            {
                PlayerTrainedRoleID = Guid.NewGuid(),
                PersonID = personID,
                PlayerPosition = position,
                PlayerRole = firstRole,
                PlayerTrainedRoleAdaptation = (byte)random.Next(80, 101) // 80-100 inclusive
            });

            // 15% chance for second trained role (50-80 adaptaption)
            if (random.Next(100) < 15)
            {
                // Make sure second role is different from first
                var availableRoles = validRoles.Where(r => r != firstRole).ToList();
                if (availableRoles.Any())
                {
                    var secondRole = availableRoles[random.Next(availableRoles.Count)];
                    trainedRoles.Add(new PlayerTrainedRole
                    {
                        PlayerTrainedRoleID = Guid.NewGuid(),
                        PersonID = personID,
                        PlayerPosition = position,
                        PlayerRole = secondRole,
                        PlayerTrainedRoleAdaptation = (byte)random.Next(50, 81) // 50-80 inclusive
                    });
                }
            }

            return trainedRoles;
        }

        private static List<PlayerRole> GetValidRolesForPosition(PlayerPosition position)
        {
            return position switch
            {
                PlayerPosition.Goalkeeper => new List<PlayerRole>
                {
                    PlayerRole.Goalkeeper,
                    PlayerRole.SweeperKeeper
                },

                PlayerPosition.RightCenterBack or
                PlayerPosition.CentralCenterBack or
                PlayerPosition.LeftCenterBack => new List<PlayerRole>
                {
                    PlayerRole.CenterBack,
                    PlayerRole.BallPlayingDefender,
                    PlayerRole.NoNonsenseCenterBack,
                    PlayerRole.Libero,
                    PlayerRole.Stopper,
                    PlayerRole.Cover
                },

                PlayerPosition.RightBack or
                PlayerPosition.LeftBack or
                PlayerPosition.RightWingBack or
                PlayerPosition.LeftWingBack => new List<PlayerRole>
                {
                    PlayerRole.FullBack,
                    PlayerRole.WingBack,
                    PlayerRole.CompleteWingBack,
                    PlayerRole.InvertedWingBack,
                    PlayerRole.WideCenterBack
                },

                PlayerPosition.RightDefensiveMidfielder or
                PlayerPosition.CentralDefensiveMidfielder or
                PlayerPosition.LeftDefensiveMidfielder => new List<PlayerRole>
                {
                    PlayerRole.DefensiveMidfielder,
                    PlayerRole.Anchorman,
                    PlayerRole.HalfBack,
                    PlayerRole.DeepLyingPlaymaker,
                    PlayerRole.Regista,
                    PlayerRole.Volante,
                    PlayerRole.SegundoVolante,
                    PlayerRole.BallWinningMidfielder
                },

                PlayerPosition.RightCenterMidfielder or
                PlayerPosition.CentralCenterMidfielder or
                PlayerPosition.LeftCenterMidfielder => new List<PlayerRole>
                {
                    PlayerRole.CentralMidfielder,
                    PlayerRole.BoxToBoxMidfielder,
                    PlayerRole.Mezzala,
                    PlayerRole.Carrilero,
                    PlayerRole.AdvancedPlaymaker,
                    PlayerRole.RoamingPlaymaker
                },

                PlayerPosition.RightMidfielder or
                PlayerPosition.LeftMidfielder or
                PlayerPosition.RightWinger or
                PlayerPosition.LeftWinger => new List<PlayerRole>
                {
                    PlayerRole.WideMidfielder,
                    PlayerRole.WidePlaymaker,
                    PlayerRole.Winger,
                    PlayerRole.InvertedWinger,
                    PlayerRole.InsideForward,
                    PlayerRole.InvertedForward,
                    PlayerRole.Raumdeuter,
                    PlayerRole.WideTargetMan,
                    PlayerRole.DefensiveWinger
                },

                PlayerPosition.RightAttackingMidfielder or
                PlayerPosition.CentralAttackingMidfielder or
                PlayerPosition.LeftAttackingMidfielder => new List<PlayerRole>
                {
                    PlayerRole.AttackingMidfielder,
                    PlayerRole.ShadowStriker,
                    PlayerRole.Enganche,
                    PlayerRole.Trequartista,
                    PlayerRole.SecondStriker,
                    PlayerRole.FalseTen,
                    PlayerRole.CentralWinger
                },

                PlayerPosition.RightStriker or
                PlayerPosition.CentralStriker or
                PlayerPosition.LeftStriker => new List<PlayerRole>
                {
                    PlayerRole.AdvancedForward,
                    PlayerRole.CompleteForward,
                    PlayerRole.Poacher,
                    PlayerRole.TargetMan,
                    PlayerRole.DeepLyingForward,
                    PlayerRole.PressingForward,
                    PlayerRole.DefensiveForward,
                    PlayerRole.FalseNine,
                    PlayerRole.TrequartistaForward
                },

                _ => new List<PlayerRole>()
            };
        }

        public async Task AssignPlayersToGeneratedTeams(IEnumerable<Guid> teamIDs)
        {
            var generatedTeamIDs = teamIDs.Distinct().ToList();
            if (!generatedTeamIDs.Any())
            {
                return;
            }

            var tactics = await _context.Tactics
                .Where(t => generatedTeamIDs.Contains(t.TeamID) && t.isMain)
                .ToListAsync();

            foreach (var tactic in tactics)
            {
                await AssignPlayersToTactic(tactic.TacticID, tactic.TeamID, tactic.Formation, Random.Shared, saveChanges: false);
            }
        }

        public async Task AssignPlayersToTactic(Guid tacticID, Guid teamID, Formation? formation)
        {
            await AssignPlayersToTactic(tacticID, teamID, formation, Random.Shared, saveChanges: true);
        }

        private async Task AssignPlayersToTactic(Guid tacticID, Guid teamID, Formation? formation, Random random, bool saveChanges)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            var teamPlayerIDs = await _context.Contracts
                .Where(c =>
                    c.TeamID == teamID &&
                    c.Role == Role.Player &&
                    (c.EndDate == null || c.EndDate > today))
                .Select(c => c.PersonID)
                .Distinct()
                .ToListAsync();

            if (!teamPlayerIDs.Any())
            {
                return;
            }

            var existingPlayerTactics = await _context.PlayerTactics
                .Where(pt => pt.TacticID == tacticID)
                .ToListAsync();

            if (existingPlayerTactics.Any())
            {
                _context.PlayerTactics.RemoveRange(existingPlayerTactics);
            }

            var assignedPlayerIDs = await AssignPlayersToFormation(tacticID, teamPlayerIDs, formation, random);
            AssignSubstitutionsAndReserves(tacticID, teamPlayerIDs, assignedPlayerIDs);

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        private async Task<HashSet<Guid>> AssignPlayersToFormation(Guid tacticID, List<Guid> teamPlayerIDs, Formation? formation, Random random)
        {
            var assignedPlayerIDs = new HashSet<Guid>();
            var slots = FormationSlotsByFormation.GetValueOrDefault(formation ?? Formation.Four_Four_Two)
                ?? FormationSlotsByFormation[Formation.Four_Four_Two];

            foreach (var slot in slots)
            {
                var playerID = await FindBestPlayerForPosition(teamPlayerIDs, assignedPlayerIDs, slot.Position, random);
                CreatePlayerTactic(tacticID, playerID, slot.Position, slot.Role);
                assignedPlayerIDs.Add(playerID);
            }

            return assignedPlayerIDs;
        }

        private void AssignSubstitutionsAndReserves(Guid tacticID, List<Guid> teamPlayerIDs, HashSet<Guid> assignedPlayerIDs)
        {
            var availablePlayers = teamPlayerIDs.Where(id => !assignedPlayerIDs.Contains(id)).ToList();
            int i = 1;
            foreach (Guid availablePlayerID in availablePlayers)
            {
                if (i <= 9)
                {
                    AddSubstitution(tacticID, availablePlayerID, i);
                }
                else
                {
                    AddReserve(tacticID, availablePlayerID);
                }

                i++;
            }
        }

        private void AddSubstitution(Guid tacticID, Guid personID, int substituteOrder)
        {
            var playerTactic = new PlayerTactic
            {
                PlayerTacticID = Guid.NewGuid(),
                TacticID = tacticID,
                PersonID = personID,
                PlayerPosition = PlayerPosition.None,
                PlayerRole = PlayerRole.None,
                SquadUnit = SquadUnit.Substitute,
                SubstituteOrder = substituteOrder
            };
            _context.PlayerTactics.Add(playerTactic);
        }

        private void AddReserve(Guid tacticID, Guid personID)
        {
            var playerTactic = new PlayerTactic
            {
                PlayerTacticID = Guid.NewGuid(),
                TacticID = tacticID,
                PersonID = personID,
                PlayerPosition = PlayerPosition.None,
                PlayerRole = PlayerRole.None,
                SquadUnit = SquadUnit.Reserve,
                SubstituteOrder = null
            };
            _context.PlayerTactics.Add(playerTactic);
        }

        private async Task<Guid> FindBestPlayerForPosition(List<Guid> teamPlayerIDs, HashSet<Guid> assignedPlayerIDs, PlayerPosition desiredPosition, Random random)
        {
            // Get all unassigned players from the team
            var availablePlayers = teamPlayerIDs.Where(id => !assignedPlayerIDs.Contains(id)).ToList();

            if (!availablePlayers.Any())
            {
                // This shouldn't happen with 30 players and 11 positions, but return random if it does
                return teamPlayerIDs[random.Next(teamPlayerIDs.Count)];
            }

            var playersWithTrainedPositions = await _context.PlayerTrainedPositions
                .Where(ptp => availablePlayers.Contains(ptp.PersonID) && ptp.PlayerPosition == desiredPosition)
                .OrderByDescending(ptp => ptp.PlayerTrainedPositionAdaptation)
                .ToListAsync();


            if (playersWithTrainedPositions.Any())
            {
                return playersWithTrainedPositions.First().PersonID;
            }

            // If no player is trained for this position, return a random available player
            return availablePlayers[random.Next(availablePlayers.Count)];
        }

        private void CreatePlayerTactic(Guid tacticID, Guid personID, PlayerPosition position, PlayerRole role)
        {
            var playerTactic = new PlayerTactic
            {
                PlayerTacticID = Guid.NewGuid(),
                TacticID = tacticID,
                PersonID = personID,
                PlayerPosition = position,
                PlayerRole = role,
                SquadUnit = SquadUnit.Starting,
                SubstituteOrder = null
            };

            _context.PlayerTactics.Add(playerTactic);
        }
    }
}
