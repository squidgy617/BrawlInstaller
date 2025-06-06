﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.StaticClasses
{
    public static class EffectPacs
    {
        public static Dictionary<string, int> FighterEffectPacs = new Dictionary<string, int> 
        {
            { "ef_common", 0 },
            { "ef_mario", 1 },
            { "ef_donkey", 2 },
            { "ef_link", 3 },
            { "ef_samus", 4 },
            { "ef_yoshi", 5 },
            { "ef_kirby", 6 },
            { "ef_fox", 7 },
            { "ef_pikachu", 8 },
            { "ef_luigi", 9 },
            { "ef_captain", 10 },
            { "ef_ness", 11 },
            { "ef_koopa", 12 },
            { "ef_peach", 13 },
            { "ef_zelda", 14 },
            { "ef_sheik", 15 },
            { "ef_popo", 16 },
            { "ef_nana", 17 },
            { "ef_marth", 18 },
            { "ef_gamewatch", 19 },
            { "ef_falco", 20 },
            { "ef_ganon", 21 },
            { "ef_wario", 22 },
            { "ef_metaknight", 23 },
            { "ef_pit", 24 },
            { "ef_szerosuit", 25 },
            { "ef_pikmin", 26 },
            { "ef_lucas", 27 },
            { "ef_diddy", 28 },
            { "ef_poketrainer", 29 },
            { "ef_pokelizardon", 30 },
            { "ef_pokezenigame", 31 },
            { "ef_pokefushigisou", 32 },
            { "ef_dedede", 33 },
            { "ef_lucario", 34 },
            { "ef_ike", 35 },
            { "ef_robot", 36 },
            { "ef_pra_mai", 37 },
            { "ef_purin", 38 },
            { "ef_mewtwo", 39 },
            { "ef_roy", 40 },
            { "ef_dr_mario", 41 },
            { "ef_toonlink", 42 },
            { "ef_toon_zelda", 43 },
            { "ef_toon_sheik", 44 },
            { "ef_wolf", 45 },
            { "ef_dixie", 46 },
            { "ef_snake", 47 },
            { "ef_sonic", 48 },
            { "ef_gkoopa", 49 },
            { "ef_StgBattleField", 50 },
            { "ef_StgFinal", 51 },
            { "ef_StgDolpic", 52 },
            { "ef_StgMansion", 53 },
            { "ef_StgMarioPast", 54 },
            { "ef_StgKart", 55 },
            { "ef_StgDonkey", 56 },
            { "ef_StgJungle", 57 },
            { "ef_StgPirates", 58 },
            { "ef_StgNorfair", 60 },
            { "ef_StgOrpheon", 61 },
            { "ef_StgCrayon", 62 },
            { "ef_StgHalberd", 63 },
            { "ef_StgStarfox", 68 },
            { "ef_StgStadium", 69 },
            { "ef_StgTengan", 70 },
            { "ef_StgFzero", 71 },
            { "ef_StgIce", 72 },
            { "ef_StgGw", 73 },
            { "ef_StgEmblem", 74 },
            { "ef_StgEmblem00", 75 },
            { "ef_StgEmblem01", 76 },
            { "ef_StgMadein", 77 },
            { "ef_StgEarth", 78 },
            { "ef_StgPalutena", 79 },
            { "ef_StgFamicom", 80 },
            { "ef_StgNewpork", 81 },
            { "ef_StgVillage", 82 },
            { "ef_StgMetalgear", 83 },
            { "ef_StgGreenhill", 84 },
            { "ef_StgPictchat", 85 },
            { "ef_StgPlankton", 86 },
            { "ef_StgDxShrine", 90 },
            { "ef_StgDxYorster", 91 },
            { "ef_StgDxGarden", 92 },
            { "ef_StgDxOnett", 93 },
            { "ef_StgDxGreens", 94 },
            { "ef_StgDxPStadium", 95 },
            { "ef_StgDxRCruise", 96 },
            { "ef_StgDxCorneria", 97 },
            { "ef_StgDxBigBlue", 98 },
            { "ef_StgDxZebes", 99 },
            { "ef_StgOldin", 100 },
            { "ef_StgHomerun", 101 },
            { "ef_StgStageedit", 102 },
            { "ef_AdvCloud", 103 },
            { "ef_AdvJungle", 104 },
            { "ef_AdvRiver", 105 },
            { "ef_AdvGrass", 106 },
            { "ef_AdvZoo", 107 },
            { "ef_AdvFortress", 108 },
            { "ef_AdvLakeside", 109 },
            { "ef_AdvCave", 110 },
            { "ef_AdvRuinfront", 111 },
            { "ef_AdvRuin", 112 },
            { "ef_AdvWild", 113 },
            { "ef_AdvCliff", 114 },
            { "ef_AdvHalberdOut", 115 },
            { "ef_AdvHalberdIn", 116 },
            { "ef_AdvAncientOut", 117 },
            { "ef_AdvFactory", 118 },
            { "ef_AdvDimension", 119 },
            { "ef_AdvStadium", 120 },
            { "ef_AdvHalberdSide", 121 },
            { "ef_AdvStore", 122 },
            { "ef_AdvFlyingPlate", 123 },
            { "ef_AdvEscape", 124 },
            { "ef_kuribo", 125 },
            { "ef_patapata", 126 },
            { "ef_hammerbros", 127 },
            { "ef_killer", 128 },
            { "ef_met", 129 },
            { "ef_karon", 130 },
            { "ef_dekakuribo", 131 },
            { "ef_blowm", 132 },
            { "ef_ploum", 133 },
            { "ef_gal", 134 },
            { "ef_galfire", 135 },
            { "ef_galice", 136 },
            { "ef_galthunder", 137 },
            { "ef_melorin", 138 },
            { "ef_popperam", 139 },
            { "ef_whauel", 140 },
            { "ef_bitan", 141 },
            { "ef_mechcannon", 142 },
            { "ef_mizuo", 143 },
            { "ef_roada", 144 },
            { "ef_bombhead", 145 },
            { "ef_blossa", 146 },
            { "ef_gyraan", 147 },
            { "ef_bucyulus", 148 },
            { "ef_tautau", 149 },
            { "ef_bubot", 150 },
            { "ef_flows", 151 },
            { "ef_aroaros", 152 },
            { "ef_botron", 153 },
            { "ef_jyakeel", 154 },
            { "ef_dyeburn", 155 },
            { "ef_torista", 156 },
            { "ef_wiiems", 157 },
            { "ef_ghamgha", 158 },
            { "ef_kyan", 159 },
            { "ef_pacci", 160 },
            { "ef_faulong", 161 },
            { "ef_deathpod", 162 },
            { "ef_byushi", 163 },
            { "ef_spar", 164 },
            { "ef_kokkon", 165 },
            { "ef_jdus", 166 },
            { "ef_arrians", 167 },
            { "ef_mite", 168 },
            { "ef_shelly", 169 },
            { "ef_ngagog", 170 },
            { "ef_gunnatter", 171 },
            { "ef_cymal", 172 },
            { "ef_teckin", 173 },
            { "ef_cataguard", 174 },
            { "ef_siralamos", 175 },
            { "ef_boobas", 176 },
            { "ef_arman", 177 },
            { "ef_prim", 178 },
            { "ef_waddledee", 179 },
            { "ef_waddledoo", 180 },
            { "ef_bladeknight", 181 },
            { "ef_brontoburt", 182 },
            { "ef_robo", 183 },
            { "ef_bonkers", 184 },
            { "ef_bosspackun", 185 },
            { "ef_rayquaza", 186 },
            { "ef_porkystatue", 187 },
            { "ef_porky", 188 },
            { "ef_galleom", 189 },
            { "ef_ridley", 190 },
            { "ef_duon", 191 },
            { "ef_metaridley", 192 },
            { "ef_taboo", 193 },
            { "ef_masterhand", 194 },
            { "ef_crazyhand", 195 },
            { "ef_falconflyer", 196 },
            { "ef_jugem", 197 },
            { "ef_goroh", 198 },
            { "ef_joe", 199 },
            { "ef_waluigi", 200 },
            { "ef_resetsan", 201 },
            { "ef_nintendogs", 202 },
            { "ef_cyborg", 203 },
            { "ef_shadow", 204 },
            { "ef_excitebike", 205 },
            { "ef_devil", 206 },
            { "ef_hmbros", 207 },
            { "ef_metroid", 208 },
            { "ef_ast_ridley", 209 },
            { "ef_wright", 210 },
            { "ef_stafy", 211 },
            { "ef_tingle", 212 },
            { "ef_katana", 213 },
            { "ef_lin", 214 },
            { "ef_customrobo", 215 },
            { "ef_littlemac", 216 },
            { "ef_tank", 217 },
            { "ef_jeff", 218 },
            { "ef_heririn", 219 },
            { "ef_barbara", 220 },
            { "ef_robin", 221 },
            { "ef_saki", 222 },
            { "ef_kururi", 223 },
            { "ef_FinMario", 224 },
            { "ef_FinDonkey", 225 },
            { "ef_FinLink", 226 },
            { "ef_FinSamus", 227 },
            { "ef_FinToonLink", 228 },
            { "ef_FinYoshi", 229 },
            { "ef_FinKirby", 230 },
            { "ef_FinFox", 231 },
            { "ef_FinPikachu", 232 },
            { "ef_FinLuigi", 233 },
            { "ef_FinCaptain", 234 },
            { "ef_FinNess", 235 },
            { "ef_FinKoopa", 236 },
            { "ef_FinPeach", 237 },
            { "ef_FinZelda", 238 },
            { "ef_FinIceclimber", 239 },
            { "ef_FinMarth", 240 },
            { "ef_FinGamewatch", 241 },
            { "ef_FinGanon", 242 },
            { "ef_FinWario", 243 },
            { "ef_FinMetaknight", 244 },
            { "ef_FinPit", 245 },
            { "ef_FinSZerosuit", 246 },
            { "ef_FinPikmin", 247 },
            { "ef_FinLucas", 248 },
            { "ef_FinDiddy", 249 },
            { "ef_FinPokeTrainer", 250 },
            { "ef_FinDedede", 251 },
            { "ef_FinLucario", 252 },
            { "ef_FinIke", 253 },
            { "ef_FinRobot", 254 },
            { "ef_FinPurin", 255 },
            { "ef_FinWolf", 256 },
            { "ef_FinSnake", 257 },
            { "ef_FinSonic", 258 },
            { "ef_advcommon", 259 },
            { "ef_pokemon", 260 },
            { "ef_KbMario", 261 },
            { "ef_KbDonkey", 262 },
            { "ef_KbLink", 263 },
            { "ef_KbSamus", 264 },
            { "ef_KbYoshi", 265 },
            { "ef_KbFox", 266 },
            { "ef_KbPikachu", 267 },
            { "ef_KbLuigi", 268 },
            { "ef_KbCaptain", 269 },
            { "ef_KbNess", 270 },
            { "ef_KbKoopa", 271 },
            { "ef_KbPeach", 272 },
            { "ef_KbZelda", 273 },
            { "ef_KbSheik", 274 },
            { "ef_KbPopo", 275 },
            { "ef_KbMarth", 276 },
            { "ef_KbGamewatch", 277 },
            { "ef_KbFalco", 278 },
            { "ef_KbGanon", 279 },
            { "ef_KbWario", 280 },
            { "ef_KbMetaknight", 281 },
            { "ef_KbPit", 282 },
            { "ef_KbSzerosuit", 283 },
            { "ef_KbPikmin", 284 },
            { "ef_KbLucas", 285 },
            { "ef_KbDiddy", 286 },
            { "ef_KbPokelizardon", 287 },
            { "ef_KbPokefushigisou", 288 },
            { "ef_KbPokezenigame", 289 },
            { "ef_KbDedede", 290 },
            { "ef_KbLucario", 291 },
            { "ef_KbIke", 292 },
            { "ef_KbRobot", 293 },
            { "ef_KbPurin", 294 },
            { "ef_KbToonlink", 295 },
            { "ef_KbWolf", 296 },
            { "ef_KbSnake", 297 },
            { "ef_KbSonic", 298 },
            { "ef_StgTarget", 299 },
            { "ef_warioman", 300 },
            { "ef_zakoboy", 301 },
            { "ef_zakogirl", 302 },
            { "ef_zakochild", 303 },
            { "ef_zakoball", 304 },
            { "ef_coinshooter", 305 },
            { "ef_cleargetter", 306 },
            { "ef_chararoll", 307 },
            { "ef_mu_seal", 308 },
            { "ef_mu_sldisplay", 309 },
            { "ef_mu_notice", 310 },
            { "ef_custom00", 311 },
            { "ef_custom01", 312 },
            { "ef_custom02", 313 },
            { "ef_custom03", 314 },
            { "ef_custom04", 315 },
            { "ef_custom05", 316 },
            { "ef_custom06", 317 },
            { "ef_custom07", 318 },
            { "ef_custom08", 319 },
            { "ef_custom09", 320 },
            { "ef_custom0A", 321 },
            { "ef_custom0B", 322 },
            { "ef_custom0C", 323 },
            { "ef_custom0D", 324 },
            { "ef_custom0E", 325 },
            { "ef_custom0F", 326 },
            { "ef_custom10", 327 },
            { "ef_custom11", 328 },
            { "ef_custom12", 329 },
            { "ef_custom13", 330 },
            { "ef_custom14", 331 },
            { "ef_custom15", 332 },
            { "ef_custom16", 333 },
            { "ef_custom17", 334 },
            { "ef_custom18", 335 },
            { "ef_custom19", 336 },
            { "ef_custom1A", 337 },
            { "ef_custom1B", 338 },
            { "ef_custom1C", 339 },
            { "ef_custom1D", 340 },
            { "ef_custom1E", 341 },
            { "ef_custom1F", 342 },
            { "ef_custom20", 343 },
            { "ef_custom21", 344 },
            { "ef_custom22", 345 },
            { "ef_custom23", 346 },
            { "ef_custom24", 347 },
            { "ef_custom25", 348 },
            { "ef_custom26", 349 },
            { "ef_custom27", 350 },
            { "ef_custom28", 351 },
            { "ef_custom29", 352 },
            { "ef_custom2A", 353 },
            { "ef_custom2B", 354 },
            { "ef_custom2C", 355 },
            { "ef_custom2D", 356 },
            { "ef_custom2E", 357 },
            { "ef_custom2F", 358 },
            { "ef_custom30", 359 },
            { "ef_custom31", 360 },
            { "ef_custom32", 361 },
            { "ef_custom33", 362 },
            { "ef_custom34", 363 },
            { "ef_custom35", 364 },
            { "ef_custom36", 365 },
            { "ef_custom37", 366 },
            { "ef_custom38", 367 },
            { "ef_custom39", 368 },
            { "ef_custom3A", 369 },
            { "ef_custom3B", 370 },
            { "ef_custom3C", 371 },
            { "ef_custom3D", 372 },
            { "ef_custom3E", 373 },
            { "ef_custom3F", 374 },
            { "ef_custom40", 375 },
            { "ef_custom41", 376 },
            { "ef_custom42", 377 },
            { "ef_custom43", 378 },
            { "ef_custom44", 379 },
            { "ef_custom45", 380 },
            { "ef_custom46", 381 },
            { "ef_custom47", 382 },
            { "ef_custom48", 383 },
            { "ef_custom49", 384 },
            { "ef_custom4A", 385 },
            { "ef_custom4B", 386 },
            { "ef_custom4C", 387 },
            { "ef_custom4D", 388 },
            { "ef_custom4E", 389 },
            { "ef_custom4F", 390 },
            { "ef_custom50", 391 },
            { "ef_custom51", 392 },
            { "ef_custom52", 393 },
            { "ef_custom53", 394 },
            { "ef_custom54", 395 },
            { "ef_custom55", 396 },
            { "ef_custom56", 397 },
            { "ef_custom57", 398 },
            { "ef_custom58", 399 },
            { "ef_custom59", 400 },
            { "ef_custom5A", 401 },
            { "ef_custom5B", 402 },
            { "ef_custom5C", 403 },
            { "ef_custom5D", 404 },
            { "ef_custom5E", 405 },
            { "ef_custom5F", 406 },
            { "ef_custom60", 407 },
            { "ef_custom61", 408 },
            { "ef_custom62", 409 },
            { "ef_custom63", 410 },
            { "ef_custom64", 411 },
            { "ef_custom65", 412 },
            { "ef_custom66", 413 },
            { "ef_custom67", 414 },
            { "ef_custom68", 415 },
            { "ef_custom69", 416 },
            { "ef_custom6A", 417 },
            { "ef_custom6B", 418 },
            { "ef_custom6C", 419 },
            { "ef_custom6D", 420 },
            { "ef_custom6E", 421 },
            { "ef_custom6F", 422 },
            { "ef_custom70", 423 },
            { "ef_custom71", 424 },
            { "ef_custom72", 425 },
            { "ef_custom73", 426 },
            { "ef_custom74", 427 },
            { "ef_custom75", 428 },
            { "ef_custom76", 429 },
            { "ef_custom77", 430 },
            { "ef_custom78", 431 },
            { "ef_custom79", 432 },
            { "ef_custom7A", 433 },
            { "ef_custom7B", 434 },
            { "ef_custom7C", 435 },
            { "ef_custom7D", 436 },
            { "ef_custom7E", 437 },
            { "ef_custom7F", 438 },
            { "ef_custom80", 439 },
            { "ef_custom81", 440 },
            { "ef_custom82", 441 },
            { "ef_custom83", 442 },
            { "ef_custom84", 443 },
            { "ef_custom85", 444 },
            { "ef_custom86", 445 },
            { "ef_custom87", 446 },
            { "ef_custom88", 447 },
            { "ef_custom89", 448 },
            { "ef_custom8A", 449 },
            { "ef_custom8B", 450 },
            { "ef_custom8C", 451 },
            { "ef_custom8D", 452 },
            { "ef_custom8E", 453 },
            { "ef_custom8F", 454 },
            { "ef_custom90", 455 },
            { "ef_custom91", 456 },
            { "ef_custom92", 457 },
            { "ef_custom93", 458 },
            { "ef_custom94", 459 },
            { "ef_custom95", 460 },
            { "ef_custom96", 461 },
            { "ef_custom97", 462 },
            { "ef_custom98", 463 },
            { "ef_custom99", 464 },
            { "ef_custom9A", 465 },
            { "ef_custom9B", 466 },
            { "ef_custom9C", 467 },
            { "ef_custom9D", 468 },
            { "ef_custom9E", 469 },
            { "ef_custom9F", 470 },
            { "ef_customA0", 471 },
            { "ef_customA1", 472 },
            { "ef_customA2", 473 },
            { "ef_customA3", 474 },
            { "ef_customA4", 475 },
            { "ef_customA5", 476 },
            { "ef_customA6", 477 },
            { "ef_customA7", 478 },
            { "ef_customA8", 479 },
            { "ef_customA9", 480 },
            { "ef_customAA", 481 },
            { "ef_customAB", 482 },
            { "ef_customAC", 483 },
            { "ef_customAD", 484 },
            { "ef_customAE", 485 },
            { "ef_customAF", 486 },
            { "ef_customB0", 487 },
            { "ef_customB1", 488 },
            { "ef_customB2", 489 },
            { "ef_customB3", 490 },
            { "ef_customB4", 491 },
            { "ef_customB5", 492 },
            { "ef_customB6", 493 },
            { "ef_customB7", 494 },
            { "ef_customB8", 495 },
            { "ef_customB9", 496 },
            { "ef_customBA", 497 },
            { "ef_customBB", 498 },
            { "ef_customBC", 499 },
            { "ef_customBD", 500 },
            { "ef_customBE", 501 },
            { "ef_customBF", 502 },
            { "ef_customC0", 503 },
            { "ef_customC1", 504 },
            { "ef_customC2", 505 },
            { "ef_customC3", 506 },
            { "ef_customC4", 507 },
            { "ef_customC5", 508 },
            { "ef_customC6", 509 },
            { "ef_customC7", 510 },
            { "ef_customC8", 511 },
            { "ef_customC9", 512 },
            { "ef_customCA", 513 },
            { "ef_customCB", 514 },
            { "ef_customCC", 515 },
            { "ef_customCD", 516 },
            { "ef_customCE", 517 },
            { "ef_customCF", 518 },
            { "ef_customD0", 519 },
            { "ef_customD1", 520 },
            { "ef_customD2", 521 },
            { "ef_customD3", 522 },
            { "ef_customD4", 523 },
            { "ef_customD5", 524 },
            { "ef_customD6", 525 },
            { "ef_customD7", 526 },
            { "ef_customD8", 527 },
            { "ef_customD9", 528 },
            { "ef_customDA", 529 },
            { "ef_customDB", 530 },
            { "ef_customDC", 531 },
            { "ef_customDD", 532 },
            { "ef_customDE", 533 },
            { "ef_customDF", 534 },
            { "ef_customE0", 535 },
            { "ef_customE1", 536 },
            { "ef_customE2", 537 },
            { "ef_customE3", 538 },
            { "ef_customE4", 539 },
            { "ef_customE5", 540 },
            { "ef_customE6", 541 },
            { "ef_customE7", 542 },
            { "ef_customE8", 543 },
            { "ef_customE9", 544 },
            { "ef_customEA", 545 },
            { "ef_customEB", 546 },
            { "ef_customEC", 547 },
            { "ef_customED", 548 },
            { "ef_customEE", 549 },
            { "ef_customEF", 550 },
            { "ef_customF0", 551 },
            { "ef_customF1", 552 },
            { "ef_customF2", 553 },
            { "ef_customF3", 554 },
            { "ef_customF4", 555 },
            { "ef_customF5", 556 },
            { "ef_customF6", 557 },
            { "ef_customF7", 558 },
            { "ef_customF8", 559 },
            { "ef_customF9", 560 },
            { "ef_customFA", 561 },
            { "ef_customFB", 562 },
            { "ef_customFC", 563 },
            { "ef_customFD", 564 },
            { "ef_customFE", 565 },
            { "ef_customFF", 566 }
        };
    }
}
