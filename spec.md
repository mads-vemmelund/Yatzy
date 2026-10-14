# Specifikation: Yatzy

## 1. Projektbeskrivelse og formål

Yatzy er en komplet, dansksproget webapplikation til 1-4 deltagere, der spiller lokalt på samme enhed. Spillet implementerer klassisk dansk Yatzy med fem seks-sidede terninger og en individuel pointtavle med 15 kategorier for hver deltager. Applikationen skal kunne bruges uden installation af klientsoftware og kunne demonstreres som afsluttende skoleprojekt.

Projektets formål er både at levere et færdigt og behageligt spil og at synliggøre centrale færdigheder i C#, objektorienteret programmering, Blazor, brugergrænseflader, inputvalidering og automatiseret test.

### Mål

- Spillet skal kunne gennemføres fra start til slut uden genindlæsning af siden.
- Alle point skal beregnes af domænelogikken, ikke af brugergrænsefladen.
- Spilregler og ugyldige handlinger skal være dækket af automatiske tests.
- Brugerfladen skal være på dansk og fungere på computer, tablet og mobil.
- Kildekode og beslutninger skal være overskuelige at forklare ved en eksamen.

### Afgrænsning og fastlagte valg

- Første version er et lokalt spil for 1-4 deltagere på samme enhed. Der er ingen netværksbaseret multiplayer, konti, servergemte spil, database eller leaderboard.
- Hver deltager vælger et navn og en farve ved spilstart. Deltagernes farver skal være forskellige, så de tydeligt kan skelnes i spillet.
- En igangværende kamp ligger i Blazor-sessionens hukommelse. Genindlæsning eller lukning af siden starter ikke automatisk det tidligere spil igen.
- Spillet følger den klassiske pointtavle med de 15 kategorier, 50 bonuspoint for mindst 63 point i øverste sektion og 50 point for Yatzy. Der gives ikke ekstra point for en eventuel ekstra Yatzy.
- Spilleren må bevidst registrere 0 i enhver ledig kategori, også selv om terningerne ellers giver en positiv score dér.
- En kategori registreres straks ved valg. Der vises ikke et separat bekræftelsestrin, heller ikke ved 0 point.

## 2. Teknologi og arkitektur

### Teknologistak

- C# på .NET 10.
- ASP.NET Core Blazor Web App med interaktiv server-rendering (`InteractiveServer`).
- Razor-komponenter til brugerfladen, HTML til semantik og CSS til layout og visuel stil.
- JavaScript må kun tilføjes, hvis en konkret browserfunktion ikke kan løses hensigtsmæssigt med Blazor og CSS. Det forventede spil kræver ikke JavaScript.
- xUnit til automatiserede tests af domænelogik og spilflow.
- Ingen ekstern database eller tredjeparts spilmotor.

### Arkitekturprincipper

Løsningen opdeles i tre projekter: web, domæne og tests. Domæneprojektet må ikke referere til Blazor, ASP.NET Core eller webprojektet. Webprojektet kalder domænelaget, viser dets tilstand og sender brugerhandlinger tilbage til domænelaget. Testprojektet refererer direkte til domænelaget og skal kunne køre uden at starte webserveren.

Tilfældige terningeværdier leveres gennem en injicerbar terningekaster. Produktionsversionen bruger en tilfældig generator; tests bruger en forudsigelig testimplementering. Dermed kan både scoring og kastforløb testes deterministisk.

Der oprettes ikke et generelt repository- eller databaseabstraktionslag, da version 1 ikke gemmer data. Domæneklasser beskytter spillets invariants, så UI'et ikke selv kan ændre registrerede point eller omgå kastgrænser.

## 3. Funktionelle krav

### FR-01: Start og nulstilling

- Før spillet startes, kan brugeren vælge 1-4 deltagere og angive navn og en forskellig farve for hver deltager.
- Startsiden viser spillets navn, deltageropsætningen og knappen **Start nyt spil**.
- Når et nyt spil startes, oprettes en tom pointtavle for hver deltager. Den første deltager får turen med fem terninger uden værdier og tre tilgængelige kast.
- Det skal være muligt at starte et nyt spil fra afslutningsskærmen uden at genindlæse siden; deltageropsætningen kan ændres inden start.

### FR-02: Terningekast

- En tur indeholder fem terninger og højst tre kast.
- Første kast kaster alle fem terninger.
- Efter et kast kan spilleren markere eller afmarkere terninger som beholdte, indtil næste kast udføres.
- Ved et nyt kast ændres værdien på alle ikke-beholdte terninger. Beholdte terninger beholder deres værdi.
- Knappen **Kast terninger** er deaktiveret, hvis turen allerede har brugt tre kast.
- UI'et viser antal resterende kast og aktuelt kastnummer.
- Efter tredje kast kan spilleren ikke ændre behold-markeringer, men kan vælge en ledig kategori.
- Spilleren kan vælge en kategori efter første, andet eller tredje kast og behøver ikke bruge alle kast.

### FR-03: Pointtavle og forslag

- Alle 15 kategorier vises under hele spillet, også før og mellem kast.
- Før første kast vises ingen scoreforslag, og kategorierne kan ikke registreres.
- Efter hvert kast beregnes et forslag for hver ledig kategori ud fra de fem aktuelle terninger.
- Forslagene er midlertidige og må ikke tælle med i den registrerede score.
- Klik på en ledig kategori registrerer straks det viste scoreforslag. Hvis forslaget er større end 0, vises også en separat **Sæt til 0**-handling for kategorien. Hvis forslaget allerede er 0, registrerer klik på kategorien 0.
- En udfyldt kategori er låst for resten af spillet og viser den registrerede score, også hvis denne er 0.
- Pointtavlen viser øverste sektions sum, bonusstatus, nederste sektions sum og samlet score.

### FR-04: Turafslutning og spilafslutning

- Deltagerne spiller i den valgte rækkefølge, én ad gangen. En deltager gennemfører sin tur ved at registrere en kategori, hvorefter turen går til den næste deltager. Efter den sidste deltager begynder en ny runde hos den første deltager.
- En tur afsluttes, når en kategori er valgt eller streget. Næste tur klargøres automatisk med fem terninger uden værdier og tre nye kast.
- Efter tredje kast kan deltageren vælge en ledig kategori med den beregnede score eller strege en valgfri ledig kategori ved at registrere 0 point.
- Spillet afsluttes automatisk, når alle deltagere har udfyldt eller streget alle 15 kategorier.
- Afslutningsskærmen viser hver deltagers øverste sum, bonus, nederste sum og endelige samlede score. Deltageren med flest point vises som vinder; ved pointlighed vises alle deltagere med den højeste score som vindere. Skærmen tilbyder handlingen **Spil igen**.

### FR-05: Status og fejl

- Ugyldige handlinger skal afvises af domænelogikken uden at ændre spiltilstanden.
- Webbrugerfladen skal forhindre eller forklare ugyldige handlinger med deaktiverede kontroller eller en kort dansk statusbesked.
- Fejl må ikke efterlade pointtavlen eller terningerne i en delvist opdateret tilstand.

## 4. Komplette spilleregler og pointberegning

Spillet anvender fem terninger med værdier fra 1 til 6. Hver af de 15 kategorier kan kun bruges én gang. Når en kategori vælges, registreres den aktuelle score; et frivilligt valg af 0 er lovligt i alle kategorier.

### 4.1 Øverste sektion

For Enere til Seksere lægges alle terninger med den valgte øjenværdi sammen. Andre terninger bidrager ikke.

| Kategori | Beregning |
|---|---|
| Enere | Antal 1'ere gange 1 |
| Toere | Antal 2'ere gange 2 |
| Treere | Antal 3'ere gange 3 |
| Firere | Antal 4'ere gange 4 |
| Femmere | Antal 5'ere gange 5 |
| Seksere | Antal 6'ere gange 6 |

Øverste sum er summen af de seks registrerede kategorier. Hvis og kun hvis alle seks kategorier er udfyldt og summen er mindst 63, gives en bonus på 50 point. En foreløbig sum undervejs er ikke i sig selv en optjent bonus. UI'et kan vise tærsklen og aktuel sum, men bonus indgår i samlet score først, når øverste sektion er fuldført og opfylder kravet.

### 4.2 Nederste sektion

| Kategori | Gyldig kombination og score |
|---|---|
| Ét par | To eller flere ens; score er summen af det højeste par. Ved tre ens vælges altså to af de ens terninger. Eksempel: 6-6-5-3-2 giver 12. |
| To par | To par med forskellige øjenværdier; score er summen af de fire terninger i de to højeste mulige forskellige par. Eksempel: 6-6-5-5-2 giver 22. Tre parværdier kan ikke forekomme med fem terninger; ved tre ens og et par tæller det højeste par af hver værdi. |
| Tre ens | Mindst tre ens; score er summen af de tre terninger i den højeste gruppe med mindst tre ens. Eksempel: 5-5-5-5-2 giver 15. |
| Fire ens | Mindst fire ens; score er summen af fire ens terninger. Eksempel: 5-5-5-5-2 giver 20. |
| Lille straight | Præcis værdierne 1, 2, 3, 4 og 5, én af hver; giver 15 point. |
| Stor straight | Præcis værdierne 2, 3, 4, 5 og 6, én af hver; giver 20 point. |
| Fuldt hus | Tre ens plus et andet par; score er summen af alle fem terninger. Fire ens plus en enkelt terning er ikke fuldt hus. Eksempel: 2-2-2-5-5 giver 16. |
| Chancen | Summen af alle fem terninger. Eksempel: 6-6-5-3-2 giver 22. |
| Yatzy | Fem ens giver 50 point; alle andre kombinationer giver 0. Ingen ekstra Yatzy-bonus eller genåbning af kategorier. |

Hvis en kombination ikke opfylder kategorien, er beregningsforslaget 0. Spilleren kan også vælge 0 med en kombination, der ellers giver point. Alle beregnere skal returnere en score mellem 0 og 50 for hver enkelt kategori; den samlede score kan være højere på grund af bonus og summering.

### 4.3 Regneeksempler

Terningerne `6-6-5-3-2` giver: Seksere 12, Ét par 12, To par 0, Tre ens 0, Fire ens 0, Lille straight 0, Stor straight 0, Fuldt hus 0, Chancen 22 og Yatzy 0. Øvrige øvre kategorier beregnes efter deres øjenværdi.

Terningerne `5-5-5-5-2` giver: Femmere 20, Ét par 10, To par 0, Tre ens 15, Fire ens 20, Fuldt hus 0, Chancen 22 og Yatzy 0. En gruppe på fire femmere er ikke et fuldt hus, da der ikke er et andet par.

Terningerne `2-2-2-5-5` giver: Toere 6, Femmere 10, Ét par 10, To par 14, Tre ens 6, Fuldt hus 16 og Chancen 16.

## 5. Brugergrænseflade og visuelt design

### 5.1 Visuel retning

- Spillefladen har et mørkegrønt kasinobord med en diskret filtstruktur, skabt med CSS-mønster eller tekstur, der ikke forstyrrer læsbarheden.
- Gyldne detaljer bruges til primære handlinger, aktiv tur og beholdte terninger. Tekst og point skal have tilstrækkelig kontrast.
- Terninger er hvide, har sorte tydelige prikker og en fast størrelse, så layoutet ikke flytter sig ved værdiskift.
- Beholdte terninger markeres både med en tydelig gylden kant og et låse-/valgt ikon eller tekstlig tilstand. Farve må ikke være det eneste signal.
- Kast kan have en kort, diskret CSS-animation. Animationen må ikke blokere gentagne handlinger eller forsinke testbar logik.
- Stilen skal føles gennemarbejdet og kasino-inspireret uden at gøre pointtavlen mindre overskuelig.

### 5.2 Skærme og komponenter

1. **Startskærm:** spilnavn, valg af 1-4 deltagere, navne- og farvevalg samt knap til at starte et spil.
2. **Spilskærm:** runde og aktiv deltager, fem terninger, kastknap, resterende kast, individuelle pointtavler og scoreoversigt.
3. **Afslutningsskærm:** hver deltagers slutresultat, vinder eller delte vindere og knap til at starte et nyt spil.

Spilskærmens centrale kontroller er:

- fem individuelle, klikbare terninger, som kan vælges/fravælges efter første og andet kast;
- kastknap med synlig information om resterende kast;
- en pointtavle opdelt i øverste og nederste sektion;
- en tydelig adskillelse mellem midlertidige forslag og permanent registrerede point;
- synlig markering af aktiv deltager og deltagerfarver, som også suppleres af navn eller anden tekstlig markering;
- mulighed for at strege en valgfri ledig kategori ved at registrere 0 point, herunder efter tredje kast;
- sum for øverste sektion, bonus, sum for nederste sektion og samlet score.

Ledige kategorier skal fremstå handlingsbare og vise forslag efter et kast. Klik på kategoriens række registrerer forslaget; ved et positivt forslag har rækken desuden en separat **Sæt til 0**-knap, som registrerer nul i stedet. Et forslag på 0 registreres ved klik på rækken. Udfyldte kategorier skal fremstå låste og vise registreret værdi. Før et kast vises kategorier som utilgængelige, ikke som valgbare 0-pointmuligheder.

### 5.3 Responsivitet og tilgængelighed

- På bred desktop-visning vises terninger/kontroller og pointtavle samtidig uden unødvendig lodret scrolling.
- På tablet tilpasses kolonnerne, og på mobil stables terningområde og pointtavle i en logisk rækkefølge.
- Alle handlinger skal kunne udføres med tastatur, have synlig fokusmarkering og forståelige tilgængelige navne.
- Semantiske knapper bruges til handlinger; klikbare terninger og kategorier skal ikke implementeres som ikke-semantiske elementer uden tastaturstøtte.
- Status efter kast, valg og fejl skal være forståelig uden alene at afhænge af animation eller farve.
- Respekter `prefers-reduced-motion` ved at fjerne eller forkorte kastanimationer.

## 6. Brugerhistorier og acceptkriterier

### US-01: Starte et spil

Som spiller vil jeg starte et nyt spil fra startsiden, så jeg kan begynde uden at konfigurere regler.

**Acceptkriterier:** Et nyt spil har 15 tomme kategorier, fem terninger uden værdi og tre tilgængelige kast. Ingen score kan registreres før første kast.

### US-02: Kaste og beholde terninger

Som spiller vil jeg kaste terninger og beholde udvalgte værdier, så jeg kan forsøge at opnå en ønsket kombination.

**Acceptkriterier:** Første kast ændrer alle fem terninger. Efter første eller andet kast kan en terning vælges eller fravælges. Ved næste kast ændres kun ikke-beholdte terninger. UI'et viser korrekt antal resterende kast.

### US-03: Vælge en kategori tidligt

Som spiller vil jeg kunne afslutte min tur efter første eller andet kast, så jeg kan vælge en score uden at bruge alle kast.

**Acceptkriterier:** Efter ethvert udført kast kan spilleren vælge en ledig kategori. Når kategorien vælges, registreres den aktuelle beregning, og næste tur klargøres.

### US-04: Se mulige point

Som spiller vil jeg se aktuelle scoreforslag for alle ledige kategorier, så jeg kan vælge informeret.

**Acceptkriterier:** Efter hvert kast opdateres alle ledige forslag ud fra de aktuelle fem terninger. Forslag påvirker ikke totalscoren, før en kategori registreres.

### US-05: Ofre en kategori

Som spiller vil jeg kunne skrive 0 i en valgfri ledig kategori, så jeg selv bestemmer, hvilken kategori jeg opgiver.

**Acceptkriterier:** Klik på en ledig kategori registrerer det aktuelle forslag. Når forslaget er positivt, kan spilleren i stedet vælge kategoriens **Sæt til 0**-knap. Når forslaget er 0, registrerer klik på kategorien 0. En registreret 0 kan skelnes fra en tom kategori.

### US-06: Se spilresultatet

Som deltager vil jeg se alles resultater, når spillet er slut, så vi kan se, hvem der har vundet, og starte igen.

**Acceptkriterier:** Når alle deltagere har udfyldt eller streget alle 15 kategorier, vises afslutningsskærmen automatisk med hver deltagers korrekte slutscore, vinderen eller delte vindere og en fungerende **Spil igen**-handling.

### US-07: Opsætte deltagere

Som arrangør vil jeg vælge 1-4 deltagere og angive navn og farve for hver, så spillet passer til gruppen og tydeligt viser, hvis tur det er.

**Acceptkriterier:** Spillet kan startes med 1-4 deltagere. Alle deltagere har et navn og en unik farve, og opsætningen vises korrekt i spillet.

### US-08: Spille på skift

Som deltager vil jeg spille min komplette tur, før turen går videre til næste deltager, så alle følger samme rækkefølge gennem spillet.

**Acceptkriterier:** Første deltager starter. Når en deltager registrerer eller streger en kategori, får næste deltager turen. Efter sidste deltager starter næste runde hos første deltager.

### US-09: Strege en kategori

Som deltager vil jeg kunne strege en valgfri ledig kategori efter mine kast ved at registrere 0 point, så jeg kan komme videre, når ingen kategori er værd at vælge.

**Acceptkriterier:** Efter et kast kan deltageren registrere 0 i en vilkårlig ledig kategori. Efter tredje kast er det fortsat muligt at vælge og strege enhver ledig kategori. En streget kategori tæller som udfyldt og kan ikke ændres.

## 7. Datamodeller og forslag til C#-klasser

Navne kan justeres under implementering, men ansvar og regler skal bevares.

### Domæneprojekt

- `ScoreCategory` (enum): præcis de 15 kategorier med stabile, entydige værdier.
- `Die`: gyldig værdi 1-6 samt om terningen er beholdt. En terning uden kastet værdi modelleres eksplicit, fx som `int?` i spiltilstanden.
- `DiceRoll`: repræsenterer de fem værdier for et udført kast og kan bruges som input til scoreberegning.
- `ScoreCalculator`: ren logik, der beregner score for én kategori eller alle kategorier ud fra præcis fem værdier.
- `ScoreEntry`: kategori, registrerede point og evt. tidspunkt/turnummer til visning eller fejlsøgning. En entry med `0` er udfyldt; fravær af entry er tom.
- `ScoreCard`: ejer én deltagers kategorier og registrerede entries og beregner øvre sum, nedre sum, bonus og total. Afviser dobbeltregistrering.
- `Player` eller tilsvarende: deltagernavn, unik visningsfarve og individuel pointtavle.
- `GameState`: spillets samlede tilstand, herunder 1-4 deltagere, aktiv deltager, rundenummer, terninger, kast i aktuel tur, aktuelle forslag og om spillet er aktivt eller afsluttet.
- `YatzyGame` eller `GameEngine`: domænefacade, der udfører handlingerne `StartNewGame`, `Roll`, `ToggleHold` og `ChooseCategory`/`CrossOutCategory`. Validerer deltagerantal, navne, farver og spilhandlinger samt opdaterer tilstanden atomisk.
- `IDiceRoller`: abstraktion for generering af værdier; implementeres af en tilfældig produktionskaster og en deterministisk testkaster.
- `GameActionResult` eller tilsvarende: resultat af en brugerhandling med succes/fejl og eventuel dansk UI-besked. Domænelaget kan alternativt bruge veldefinerede exceptions, men UI'et må ikke bruge exceptions som normal kontrolflow.

### UI-projekt

- `App.razor` og router: applikationsskal og navigation.
- `Home.razor`: startsiden.
- `Game.razor`: spilskærm med aktiv deltager, individuelle pointtavler og binding til en scoped `YatzyGame`/spil-session.
- `GameOver.razor` eller en afslutningskomponent: slutresultat og genstart.
- `DiceTray.razor` og `DieView.razor`: terningvisning og behold-handling.
- `ScoreCardView.razor`: pointtavle, forslag, registrerede værdier og summer.
- Deltageropsætning og resultatvisning: valg af antal deltagere, navne og farver samt slutresultat og vinder.
- `ScoreCategoryRow.razor`: tilgængelig kategori-række med handlingsnavn, scoreforslag og registreret/låst tilstand.
- CSS opdeles efter eksisterende Blazor-templatekonventioner og komponentbehov; styling af terninger og pointtavle holdes adskilt fra domænekode.

Spiltilstanden er scoped til den aktuelle Blazor-circuit. Komponenter skal ikke oprette deres egen parallelle pointlogik; alle handlinger går gennem domænefacaden.

## 8. Spiltilstande og flow

### Tilstande

- `NotStarted`: ingen aktiv kamp; startsiden vises.
- `SettingUpPlayers`: valg af deltagerantal, navne og farver før spilstart.
- `AwaitingFirstRoll`: aktiv kamp, ingen terninger kastet i turen, tre kast til rådighed.
- `ChoosingAfterRoll`: mindst ét kast udført; spiller kan vælge beholdte terninger, kaste igen eller registrere kategori.
- `GameOver`: alle kategorier er udfyldt; kun resultat og genstart er tilgængeligt.

`ChoosingAfterRoll` behøver ikke være en separat lagret enum-værdi, hvis tilstanden entydigt kan udledes af kasttæller og terningværdier. Uanset repræsentation skal de nævnte overgangsregler håndhæves.

### Normal tur

1. Før en kamp vælges 1-4 deltagere, og hver deltager får et navn og en unik farve.
2. Ny kamp opretter en tom pointtavle for hver deltager og fem ikke-kastede terninger til første deltager.
3. Første kast genererer værdier for alle terninger og sætter kastforbruget til 1.
4. Domænelaget beregner scoreforslag for alle ledige kategorier på den aktive deltagers pointtavle.
5. Deltageren kan ændre behold-markeringer.
6. Et nyt kast bruger én kastmulighed og genererer nye værdier for ikke-beholdte terninger. Beholdte værdier er uændrede.
7. Deltageren kan gentage valget/kastet, indtil tre kast er brugt, eller vælge en kategori tidligere. En vilkårlig ledig kategori kan også streges ved at registrere 0 point.
8. Valg eller overstregning af en kategori registrerer scoren, nulstiller turens terninger og kasttæller og giver turen til næste deltager i rækkefølgen.
9. Efter sidste deltager begynder næste runde hos første deltager. Når alle deltagere har udfyldt alle 15 kategorier, går spillet til `GameOver` og viser slutresultaterne sorteret efter score.

### Invariants

- En kamp har mellem 1 og 4 deltagere; hver deltager har præcis én individuel pointtavle.
- Alle deltagere har et navn og en farve, og farverne er unikke i samme kamp.
- Der er altid præcis fem terninger i en aktiv deltagers tur.
- Kasttæller er 0 ved ny tur og højst 3.
- Første kast udføres med alle fem terninger; et senere kast ændrer aldrig værdien af en beholdt terning.
- Pointforslag kan kun eksistere efter mindst ét kast i den aktuelle tur.
- Kun ledige kategorier kan vælges; registrerede værdier kan aldrig ændres.
- Hver deltagers samlede score består af den deltagers registrerede kategorier og optjente bonus, aldrig af midlertidige forslag.
- Turen går videre i fast rækkefølge efter hver registrerede eller overstregede kategori.
- Spillet afsluttes præcis, når alle deltagere har registreret eller streget alle 15 kategorier.
- Vinderen er deltageren eller deltagerne med den højeste samlede score.

## 9. Fejlhåndtering og validering

Domænelaget skal afvise følgende uden at ændre tilstanden:

- kast før et nyt spil er startet;
- kast efter tredje kast i samme tur;
- behold/fravælg af en terning, før den har en værdi, eller efter tredje kast;
- registrering af kategori før første kast i turen;
- registrering af en allerede udfyldt kategori;
- terningeværdi uden for intervallet 1-6 eller scoreberegning på et forkert antal terninger;
- start med færre end 1 eller flere end 4 deltagere, manglende deltager-navn eller gentagne deltagerfarver;
- handlinger efter spilafslutning, bortset fra start af et nyt spil.

UI'et skal holde visning og domænetilstand synkroniseret efter succesfulde handlinger. Fejlbeskeder skal være korte, danske og konkrete, fx at alle kast er brugt, eller at kategorien allerede er valgt. Der må ikke vises tekniske stack traces til spilleren.

## 10. Ikke-funktionelle krav

- **Korrekthed:** Scoring følger afsnit 4; domænelogikken er autoritativ.
- **Testbarhed:** Scoring og spilflow kan testes uden browser, netværk eller webserver.
- **Vedligeholdelse:** Klasser har ét tydeligt ansvar; kategorier repræsenteres centralt, så UI og regler ikke divergerer.
- **Responsivitet:** Spillet kan bruges ved almindelige desktop-, tablet- og mobilbredder uden overlappende indhold eller vandret scrolling på siden.
- **Tilgængelighed:** Tastaturbetjening, fokusmarkering, kontrast og tekstlige statusindikatorer indgår fra starten.
- **Ydeevne:** En scoreberegning er lille og synkron; brugerhandlinger skal opdatere visningen uden mærkbar ventetid.
- **Stabilitet:** Ugyldige handlinger ændrer ikke delvist spilletilstanden.
- **Sikkerhed:** Ingen brugeroprettede konti eller persistente brugerdata i version 1. Standard ASP.NET Core-sikkerhedsindstillinger og HTTPS anvendes ved hosting.
- **Sprog:** Al brugerrettet tekst er dansk; kodeidentifikatorer kan være engelske for at følge almindelig C#-konvention.

## 11. Teststrategi

Testprojektet bruger xUnit og refererer til domæneprojektet. Terningekasteren erstattes i tests med en deterministisk implementering, så testene ikke afhænger af tilfældige resultater.

### Scoreberegning

- Test hver af de seks øvre kategorier med 0, én og flere matchende terninger.
- Test Ét par vælger højeste par, også når terningerne indeholder to forskellige par.
- Test To par kræver to forskellige par og summerer de to højeste gyldige par.
- Test Tre ens og Fire ens med både præcis og større gruppe, herunder fire ens til Tre ens.
- Test Lille straight og Stor straight med præcis gyldig sekvens samt ugyldige sekvenser, dubletter og manglende værdier.
- Test Fuldt hus med gyldigt 3+2, ugyldigt 4+1 og ugyldigt 3+1+1.
- Test Chancen summerer alle fem værdier.
- Test Yatzy giver 50 alene ved fem ens.
- Test eksemplerne fra afsnit 4.3.
- Test inputvalidering for forkert antal terninger og værdier uden for 1-6.

### Pointtavle og bonus

- Test bonus præcis ved 63 og over 63.
- Test ingen bonus under 63.
- Test bonus først indgår, når alle seks øvre kategorier er udfyldt.
- Test at registreret 0 tæller som udfyldt, ikke som tom.
- Test at hver deltagers pointtavle og score beregnes uafhængigt.
- Test at en kategori ikke kan registreres to gange.
- Test samlet score er summen af registrerede entries og optjent bonus, uden scoreforslag.

### Kast og spilflow

- Test starttilstand og første kast med fem værdier.
- Test at højst tre kast kan udføres pr. tur, og at fjerde kast afvises uden ændring.
- Test at beholdte terninger ikke ændrer værdi ved senere kast.
- Test at behold kan vælges/fravælges efter første og andet kast og afvises efter tredje.
- Test kategori før første kast afvises.
- Test valg efter første og andet kast samt efter tredje kast.
- Test kategori-valg låser værdien, nulstiller turen og klargør fem nye ikke-kastede terninger.
- Test at spillet afsluttes efter præcis 15 udfyldte kategorier, og at yderligere spilhandlinger afvises.
- Test at start af et nyt spil efter afslutning giver ren pointtavle og ny tur.
- Test at spillet afviser deltagerantal uden for 1-4 samt ugyldige navne eller gentagne farver.
- Test at turen starter hos første deltager og går videre til næste efter kategori-valg eller overstregning, også fra sidste deltager tilbage til første.
- Test at en valgfri ledig kategori kan overstreges for 0 efter tredje kast, og at den derefter er låst.
- Test at spillet først afsluttes, når alle deltagere har udfyldt alle 15 kategorier.
- Test at slutresultater sorteres korrekt, at højeste score vinder, og at pointlighed giver delte vindere.

### UI-kontrol

Manuel accepttest kontrollerer tastaturbetjening, fokus, kontrast, mobil/tablet-layout, at pointforslag er adskilt fra registrerede værdier, og at knappernes deaktiverede tilstande følger domænetilstanden. En senere iteration kan supplere med komponent- eller browserintegrationstests, men det er ikke en forudsætning for første version.

## 12. Projektstruktur

```text
Yatzy/
|-- Yatzy.sln
|-- spec.md
|-- src/
|   |-- Yatzy.Core/
|   |   |-- Yatzy.Core.csproj
|   |   |-- Models/
|   |   |-- Scoring/
|   |   |-- Game/
|   |   `-- Randomness/
|   `-- Yatzy.Web/
|       |-- Yatzy.Web.csproj
|       |-- Components/
|       |   |-- Layout/
|       |   |-- Pages/
|       |   |-- Game/
|       |   `-- ScoreCard/
|       |-- wwwroot/css/
|       `-- Program.cs
`-- tests/
    `-- Yatzy.Tests/
        |-- Yatzy.Tests.csproj
        |-- Scoring/
        |-- Game/
        `-- ScoreCard/
```

Afhængighedsretning: `Yatzy.Web` refererer til `Yatzy.Core`; `Yatzy.Tests` refererer til `Yatzy.Core`. Domænet refererer ikke til web eller tests.

## 13. Dokumentation til skoleprojektet

README skal efter implementeringen beskrive forudsætninger, hvordan løsningen startes, hvordan tests køres, projektstrukturen og spillets regler. En teknisk gennemgang skal forklare:

- hvordan klasser, objekter, properties og metoder bruges i spiltilstanden;
- hvordan enum og collections repræsenterer kategorier og pointtavle;
- hvordan betingelser og løkker anvendes i scoring og validering;
- hvorfor scoreberegning ligger i et separat domænelag;
- hvordan kastgeneratoren gør tests deterministiske;
- hvordan xUnit-tests dækker pointregler, turflow og ugyldige handlinger;
- hvordan Blazor-komponenter viser tilstand uden at eje forretningsregler.

De vigtigste tekniske beslutninger og deres begrundelser skal holdes opdateret, især hvis implementeringen afviger fra denne specifikation.

## 14. Trinvis implementeringsplan

1. Opret solution og projekterne `Yatzy.Core`, `Yatzy.Web` og `Yatzy.Tests`; fastlæg projektreferencer og .NET 10.
2. Implementer kategorimodel, terningmodel, `ScoreCalculator` og tests for samtlige 15 kategorier.
3. Implementer `ScoreCard`, registrering, summer, bonus og tests for grænsetilfælde.
4. Implementer `YatzyGame`/`GameEngine`, tilstandsovergang, kastbegrænsning, beholdte terninger og deterministic testkaster; test ugyldige og gyldige flows.
5. Opret Blazor-startskærm og en scoped spil-session, der kalder domænelaget.
6. Implementer terningebakke, kastkontroller, behold-markeringer, scoretavle og tydelig forskel på forslag og registrerede point.
7. Implementer automatisk turstart, slutskærm og genstart uden sidegenindlæsning.
8. Tilføj kasino-inspireret, responsivt CSS-design, tastaturbetjening, fokus, kontrast og reduceret bevægelse.
9. Kør hele xUnit-testsuiten, verificér `dotnet build`, gennemfør manuelle desktop- og mobilaccepttests, og ret uoverensstemmelser med specifikationen.
10. Færdiggør README og den tekniske forklaring til skolefremlæggelsen.

## 15. Definition af færdig

Version 1 er færdig, når alle funktionelle krav er opfyldt, alle 15 kategorier og bonus har automatiseret testdækning, ugyldige handlinger afvises uden tilstandsskade, et spil kan gennemføres og genstartes uden genindlæsning, og brugerfladen fungerer på desktop og mobil med synlige og forståelige spiltilstande.