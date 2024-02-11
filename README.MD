Bahasa Pemrograman yang dipakai:
  C#, .NET CORE 6

Database yang digunakan:
  SQL SERVER EXPRESS 2019

Untuk menjalankan aplikasi ini:
  1. Install .NET 6
  2. Install SQL SERVER EXPRESS
    - PENTING!! COPY CONNECTION STRING YANG DIMUNCULKAN DI AKHIR INSTALASI
  3. CONFIG SQL SERVER EXPRESS:
    - Buka SQL SERVER CONFIGURATION MANAGER
    - Di Config Tree sebelah kiri, cari SQL Server Network Configuration, expand menu tersebut.
    - Klik Protocols for SQLEXPRESS
    - di kolom protocol name, double klik TCP/IP, akan muncul menu configurasi TCP/IP
    - di tab Protocol, pastikan field Enabled menjadi Yes
    - di tab IP Addresses, scroll ke paling bawah, cari TCP Dynamic Ports atau TCP Port
    - secara default port yang digunakan adalah 1433, jika kedua field (TCP Dynamic Ports dan TCP Port) kosong artinya menggunakan port 1433
    - jika salah satu field mempunyai nilai, catat nomor port tersebut. 
    - kembali ke Config Tree, buka SQL Server Services
    - klik kanan SQL Server (SQLEXPRESS) lalu pilih Restart
    - setelah restart service berhasil, cek kembalik nomor port untuk TCP/IP nya.

  4. clone repository ini, baik menggunakan GIT CLONE atau download repository ini menjadi ZIP (jika menggunakan zip, extract repository terlebih dahulu)
  5. masuk ke direktory TEST_BE_79_RAKA
  6. buka/edit file appsettings.json
  7. ganti nilai dari WindowsConnection menjadi ConnectionString yang didapat dari step no. 2
  8. jika pada step 3. port tidak sesuai, maka masukkan juga no portnya seperti:

      "Server=localhost\SQLEXPRESS[PORT];Database=master;Trusted_Connection=True;"
        contoh:
      "Server=localhost\SQLEXPRESS,3503;Database=master;Trusted_Connection=True;"

     kemudian save file tersebut.
  9. klik kanan file run.cmd, pilih edit atau buka dengan text editor (misal notepad)
  10. di baris kedua, samakan port yang digunakan oleh SQL SERVER:
      sqlcmd -S tcp:localhost,[PORT] -i Data\init.sql
        contoh:
      sqlcmd -S tcp:localhost,3503 -i Data\init.sql

      kemudian save file tersebut.

  11. untuk menjalankan applikasi ini cukup dengan menjalankan run.cmd dengan cara double klik. 
      atau dengan cara:
      - buka CMD/SHELL
      - change directory ke TEST_BE_79_RAKA (cd [dir_to_repository]\TEST_BE_79_RAKA)
      - inisiasi database dengan perintah:
        sqlcmd -S tcp:localhost,[PORT] -i Data\init.sql
      - jalankan applikasi dengan perintah:
        dotnet run

API DOC:
setelah applikasi berhasil dijalankan silahkan ke halaman berikut untuk melihat dokumentasinya:
  http://localhost:5151/swagger
