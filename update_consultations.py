#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import re

file_path = r'C:\Users\Guilherme Maciel\RiderProjects\2026-ei-engsof-bc4\BlazorProject\Components\Pages\Consultations.razor'

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# First replacement - add the openForm parameter
pattern1 = r'(    public string\? PatientIdFromQuery \{ get; set; \}\s*\n\s*\n)(     private bool DialogOpen;)'
replacement1 = r'''\1     [Parameter]
     [SupplyParameterFromQuery(Name = "openForm")]
     public string? OpenFormFromQuery { get; set; }

\2'''

content = re.sub(pattern1, replacement1, content)

# Second replacement - add the check in OnInitializedAsync
pattern2 = r'(        if \(!string\.IsNullOrWhiteSpace\(PatientIdFromQuery\)\)\s*\n\s*\{\s*\n\s*FilterPatient = PatientIdFromQuery;\s*\n\s*\}\s*\n\s*\n)(        await LoadPageDataAsync\(\);)'
replacement2 = r'''\1        if (OpenFormFromQuery == "true")
        {
            DialogOpen = true;
        }

\2'''

content = re.sub(pattern2, replacement2, content)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Ficheiro atualizado com sucesso!")
