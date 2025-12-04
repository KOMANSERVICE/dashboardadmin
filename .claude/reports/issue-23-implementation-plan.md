# Plan d'implémentation - Issue #23 : Utilisation package IDR.Library.Blazor

## Résumé
Cette issue demande de remplacer les composants dans FrontendAdmin.Shared par ceux du package IDR.Library.Blazor et de signaler les composants manquants.

## État actuel
- **IDR.Library.Blazor v3.0.3** est déjà utilisé dans le projet
- Plusieurs composants IDR sont déjà importés et utilisés
- Certains composants personnalisés peuvent être remplacés par des équivalents IDR
- Plusieurs composants manquent dans IDR.Library.Blazor

## Actions à effectuer

### 1. Remplacements de composants (Immédiat)

#### a) InputCheckbox → Checkbox
- **Fichier**: `Pages/Auths/Login.razor`
- **Action**: Remplacer `<InputCheckbox>` par `<Checkbox>` de IDR.Library.Blazor.Inputs

#### b) LoadingData → Spinner ou Skeleton
- **Fichier**: `Pages/Applications/ListApp.razor`
- **Action**: Remplacer `<LoadingData />` par `<Spinner>` ou `<Skeleton>` selon le besoin

### 2. Création d'issues dans KOMANSERVICE/IDR.Library

#### Issue 1: [Component] IdrPageTitle (Priorité: Haute)
**Justification**: Utilisé 3+ fois dans le projet
**Spécifications suggérées**:
- Title (string, required)
- Subtitle (string, optional)
- Icon (string, optional)
- CssClass (string, optional)

#### Issue 2: [Component] IdrResponsiveDataTable (Priorité: Haute)
**Justification**: Composant complexe réutilisable
**Spécifications suggérées**:
- Items (IEnumerable<T>, required)
- DesktopColumnCount (int)
- EmptyStateText (string)
- Slots: Toolbar, DesktopHeader, RowTemplate, MobileCardTemplate, Footer
- Support générique <T>

#### Issue 3: [Component] IdrInfoCard (Priorité: Moyenne)
**Justification**: Carte d'information réutilisable
**Spécifications suggérées**:
- Title (string, required)
- Subtitle (string, optional)
- IconCss (string, optional)
- MetadataLabel/Value (string, optional)
- LinkHref (string, optional)
- HeaderActions (RenderFragment, optional)

#### Issue 4: [Component] IdrActionCard (Priorité: Moyenne)
**Justification**: Carte d'action réutilisable
**Spécifications suggérées**:
- Title (string, required)
- Description (string, optional)
- IconCss (string, optional)
- OnClick (EventCallback)

#### Issue 5: [Component] IdrHeaderPage (Priorité: Moyenne)
**Justification**: En-tête de page standardisé
**Spécifications suggérées**:
- Title (string, required)
- SubTitle (string, optional)
- Actions (RenderFragment, optional)

#### Issue 6: [Component] IdrIcon (Priorité: Faible)
**Justification**: Composant d'icône générique
**Spécifications suggérées**:
- Support SVG inline
- Support font icons (FontAwesome, etc.)
- Size (IconSize enum)
- Color (string)

## Séquence d'implémentation

1. **Phase 1**: Remplacements immédiats
   - Remplacer InputCheckbox par Checkbox
   - Remplacer LoadingData par Spinner/Skeleton
   - Tester les modifications

2. **Phase 2**: Création des issues
   - Créer les 6 issues dans le repo KOMANSERVICE/IDR.Library
   - Ajouter au project board #4

3. **Phase 3**: Post mise à jour IDR.Library
   - Attendre la création des nouveaux composants
   - Remplacer les composants locaux par les nouveaux composants IDR

## Tests à effectuer
- Vérifier que les remplacements n'affectent pas les fonctionnalités
- Tester sur toutes les plateformes (MAUI, Web, WASM)
- Valider les bindings et événements

## Notes
- Ne pas supprimer les composants personnalisés avant que les équivalents IDR soient disponibles
- Documenter les changements pour l'équipe
- Prévoir une migration progressive après chaque release d'IDR.Library