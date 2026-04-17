mkdir test1st
cd test1st
New-Item test_11.txt
New-Item test_12.txt
New-Item test_13.txt
New-Item work_11.txt
New-Item work_12.txt
New-Item work_13.txt
mkdir test2nd
cd test2nd
New-Item test_21.txt
New-Item test_22.txt
New-Item test_23.txt
New-Item work_21.txt
New-Item work_22.txt
New-Item work_23.txt
cd ../..
Write-Host "`r`n --- Test Environment ---`n" -ForeGround Cyan
tree /f test1st
