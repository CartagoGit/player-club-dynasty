/**
 * tools/contracts/constants/reference/dinamic-catalogue.constants.ts
 *
 * The registry of classic titles we study to build PC Dynasty's
 * interface grammar, and where their disk images live.
 *
 * **Why a registry instead of a README with links.** Answering "how did
 * the original behave here?" by running it beats guessing, and the wiki
 * says so ([`docs/wiki/pc-dynasty/como-mirar-el-juego.md`]). But the
 * images are gigabytes and the archive rate-limits, so the fetch has to
 * be resumable, verifiable and repeatable — which needs the sizes and
 * the URLs as data, not as prose.
 *
 * **Nothing here enters the repository.** The downloads land in
 * `.generate/reference/`, which is gitignored. These are references, the
 * same as the screenshots catalogued in `docs/wiki/pcfutbol/14`: we look
 * at them, we describe what we see, and we commit only our own words and
 * our own wireframes.
 *
 * **Each disk slot.** `hda` is a bootable system image with the game
 * already installed — that is what makes these usable, because a game CD
 * on its own has no operating system to run on. `hdb` is its data
 * volume, `cdrom` the original disc. A title with no `hdb` keeps
 * everything on `hda`.
 *
 * ## Game ids
 *
 * Pass one to `bun run download --game=<id>`:
 *
 *   pcf7             PC Fútbol 7.0 (1998)
 *   pcf6             PC Fútbol 6.0 (1997)
 *   pcbasket65       PC Basket 6.5 (1999)
 *   pcf5             PC Fútbol 5.0 (1996)
 *   pcf4             PC Fútbol 4.0 (1995)
 *   pcf6arg          PC Fútbol 6.0 · Apertura '98 (Argentina) (1998)
 *   pcf5arg          PC Fútbol 5.0 · Apertura '97 (Argentina) (1997)
 *   pcfa96           PC Fútbol 4.0 · Apertura '96 (Argentina) (1996)
 *   euro96           PC Selección Española de Fútbol Eurocopa '96 (1996)
 *   wc98             PC Selección Española de Fútbol Mundial '98 (1998)
 *   combatchess      Combat Chess (1997)
 *   eurotour         EuroTour Cycling (2001)
 *   igor             Igor: Objetivo Uikokahonia (1994)
 *   atm2000          PC Atlético de Madrid 2000 (2000)
 *   pcbarca99        PC Barça '99 (1999)
 *   pcbarcabasket99  PC Barça Basket '99 (1999)
 *   pcbasket         PC Basket 4.0 (1996)
 *   pccalcio         PC Calcio 4.0 (1996)
 *   pccalcio4es      PC Calcio 4.0 (Edición Española) (1996)
 *   pccalcio5        PC Calcio 5 (Italia) (1997)
 *   pccalcio5es      PC Calcio 5.0 (Versión Española) (1997)
 *   pccalcio6        PC Calcio 6 (Italia) (1998)
 *   pccalcio6es      PC Calcio 6.0 (Versión Española) (1998)
 *   pccalcio7        PC Calcio 7 (Italia) (1999)
 *   pcfrance         PC France 5.0 (1997)
 *   pcpremier5       PC Premier 5.0 (1997)
 *   pcpremier        PC Premier 6.0 (1998)
 *   pcrm             PC Real Madrid 99 (1999)
 *   pcrmbasket99     PC Real Madrid Basket '99 (1999)
 *   pcriver          PC River (2000)
 *   pm97             Premier Manager 97 (1997)
 *   pm98             Premier Manager 98 (1998)
 *   pm99             Premier Manager 99 (1999)
 *
 * Source: the public registry of the preservation archive at
 * <https://online.dinamicmultimedia.es/>. Refresh it, and find out
 * whether anything drifted, with `bun run download:catalog`.
 */

import type { IReferenceGame } from '../../interfaces/reference/reference-game.interface.ts';

/** Where the archive serves the disk images from. */
export const DISC_HOST = 'https://discos.dinamicmultimedia.es';

/** Everything the downloader knows how to fetch, indexed by game id. */
export const REFERENCE_GAMES = {
	/** PC Fútbol 7.0 (1998) */
	pcf7: {
		name: "PC Fútbol 7.0",
		year: '1998',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pcf7.bin?v=2`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pcf7_data.bin`, size: 1073741824 },
			cdrom: { url: `${DISC_HOST}/PCF7.bin`, size: 691337216 },
		},
	},
	/** PC Fútbol 6.0 (1997) */
	pcf6: {
		name: "PC Fútbol 6.0",
		year: '1997',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pcf6.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pcf6_data.bin`, size: 751619072 },
			cdrom: { url: `${DISC_HOST}/PCF6.bin`, size: 671795200 },
		},
	},
	/** PC Basket 6.5 (1999) */
	pcbasket65: {
		name: "PC Basket 6.5",
		year: '1999',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pcb65.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pcb65_data.bin`, size: 536870912 },
			cdrom: { url: `${DISC_HOST}/PCB65.bin`, size: 691337216 },
		},
	},
	/** PC Fútbol 5.0 (1996) */
	pcf5: {
		name: "PC Fútbol 5.0",
		year: '1996',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pcf5.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pcf5_data.bin`, size: 536870912 },
			cdrom: { url: `${DISC_HOST}/PCF5.bin`, size: 447692800 },
		},
	},
	/** PC Fútbol 4.0 (1995) */
	pcf4: {
		name: "PC Fútbol 4.0",
		year: '1995',
		disks: {
			hda: { url: `${DISC_HOST}/pcf4_disq.bin`, size: 523837440 },
		},
	},
	/** PC Fútbol 6.0 · Apertura '98 (Argentina) (1998) */
	pcf6arg: {
		name: "PC Fútbol 6.0 · Apertura '98 (Argentina)",
		year: '1998',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pcf6arg.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pcf6arg_data.bin`, size: 429496320 },
			cdrom: { url: `${DISC_HOST}/PCF6ARG.bin`, size: 690288640 },
		},
	},
	/** PC Fútbol 5.0 · Apertura '97 (Argentina) (1997) */
	pcf5arg: {
		name: "PC Fútbol 5.0 · Apertura '97 (Argentina)",
		year: '1997',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pcf5arg.bin`, size: 523837440 },
			cdrom: { url: `${DISC_HOST}/PCF5ARG.bin`, size: 401160192 },
		},
	},
	/** PC Fútbol 4.0 · Apertura '96 (Argentina) (1996) */
	pcfa96: {
		name: "PC Fútbol 4.0 · Apertura '96 (Argentina)",
		year: '1996',
		disks: {
			hda: { url: `${DISC_HOST}/pcfa96_boot.bin`, size: 523837440 },
		},
	},
	/** PC Selección Española de Fútbol Eurocopa '96 (1996) */
	euro96: {
		name: "PC Selección Española de Fútbol Eurocopa '96",
		year: '1996',
		disks: {
			hda: { url: `${DISC_HOST}/euro96_boot.bin`, size: 523837440 },
		},
	},
	/** PC Selección Española de Fútbol Mundial '98 (1998) */
	wc98: {
		name: "PC Selección Española de Fútbol Mundial '98",
		year: '1998',
		disks: {
			hda: { url: `${DISC_HOST}/win98_wc98.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_wc98_data.bin`, size: 268435456 },
			cdrom: { url: `${DISC_HOST}/WC98.bin`, size: 569686016 },
		},
	},
	/** Combat Chess (1997) */
	combatchess: {
		name: "Combat Chess",
		year: '1997',
		disks: {
			hda: { url: `${DISC_HOST}/win98_combatchess.bin`, size: 523837440 },
			cdrom: { url: `${DISC_HOST}/combatchess_cd.bin`, size: 639590400 },
		},
	},
	/** EuroTour Cycling (2001) */
	eurotour: {
		name: "EuroTour Cycling",
		year: '2001',
		disks: {
			hda: { url: `${DISC_HOST}/win98_eurotour.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_eurotour_data.bin`, size: 536870912 },
			cdrom: { url: `${DISC_HOST}/EUROTOUR.bin`, size: 281292800 },
		},
	},
	/** Igor: Objetivo Uikokahonia (1994) */
	igor: {
		name: "Igor: Objetivo Uikokahonia",
		year: '1994',
		disks: {
			hda: { url: `${DISC_HOST}/igor_boot.bin`, size: 523837440 },
			cdrom: { url: `${DISC_HOST}/igor_cd.bin`, size: 264189952 },
		},
	},
	/** PC Atlético de Madrid 2000 (2000) */
	atm2000: {
		name: "PC Atlético de Madrid 2000",
		year: '2000',
		disks: {
			hda: { url: `${DISC_HOST}/win98_atm2000.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_atm2000_data.bin`, size: 536870912 },
			cdrom: { url: `${DISC_HOST}/ATM2000.bin`, size: 690270208 },
		},
	},
	/** PC Barça '99 (1999) */
	pcbarca99: {
		name: "PC Barça '99",
		year: '1999',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pcbarca99.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pcbarca99_data.bin`, size: 429496320 },
			cdrom: { url: `${DISC_HOST}/PCBARCA99.bin`, size: 675543040 },
		},
	},
	/** PC Barça Basket '99 (1999) */
	pcbarcabasket99: {
		name: "PC Barça Basket '99",
		year: '1999',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pcbarcabasket99.bin`, size: 523837440 },
			cdrom: { url: `${DISC_HOST}/PCBB99.bin`, size: 626370560 },
		},
	},
	/** PC Basket 4.0 (1996) */
	pcbasket: {
		name: "PC Basket 4.0",
		year: '1996',
		disks: {
			hda: { url: `${DISC_HOST}/pcbasket45_boot.bin`, size: 523837440 },
		},
	},
	/** PC Calcio 4.0 (1996) */
	pccalcio: {
		name: "PC Calcio 4.0",
		year: '1996',
		disks: {
			hda: { url: `${DISC_HOST}/pccalcio_boot.bin`, size: 523837440 },
		},
	},
	/** PC Calcio 4.0 (Edición Española) (1996) */
	pccalcio4es: {
		name: "PC Calcio 4.0 (Edición Española)",
		year: '1996',
		disks: {
			hda: { url: `${DISC_HOST}/calcio4es_boot.bin`, size: 523837440 },
			cdrom: { url: `${DISC_HOST}/calcio4es_cd.bin`, size: 41299968 },
		},
	},
	/** PC Calcio 5 (Italia) (1997) */
	pccalcio5: {
		name: "PC Calcio 5 (Italia)",
		year: '1997',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pccalcio5.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pccalcio5_data.bin`, size: 214748160 },
			cdrom: { url: `${DISC_HOST}/PCCALCIO5.bin`, size: 429889536 },
		},
	},
	/** PC Calcio 5.0 (Versión Española) (1997) */
	pccalcio5es: {
		name: "PC Calcio 5.0 (Versión Española)",
		year: '1997',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pccalcio5es.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pccalcio5es_data.bin`, size: 214748160 },
			cdrom: { url: `${DISC_HOST}/pccalcio5es_cd.bin`, size: 441139200 },
		},
	},
	/** PC Calcio 6 (Italia) (1998) */
	pccalcio6: {
		name: "PC Calcio 6 (Italia)",
		year: '1998',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pccalcio6.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pccalcio6_data.bin`, size: 429496320 },
			cdrom: { url: `${DISC_HOST}/PCCALCIO6.bin`, size: 476594176 },
		},
	},
	/** PC Calcio 6.0 (Versión Española) (1998) */
	pccalcio6es: {
		name: "PC Calcio 6.0 (Versión Española)",
		year: '1998',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pccalcio6es.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pccalcio6es_data.bin`, size: 429496320 },
			cdrom: { url: `${DISC_HOST}/pccalcio6es_cd.bin`, size: 384614400 },
		},
	},
	/** PC Calcio 7 (Italia) (1999) */
	pccalcio7: {
		name: "PC Calcio 7 (Italia)",
		year: '1999',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pccalcio7.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pccalcio7_data.bin`, size: 858993152 },
			cdrom: { url: `${DISC_HOST}/PCCALCIO7.bin`, size: 691337216 },
		},
	},
	/** PC France 5.0 (1997) */
	pcfrance: {
		name: "PC France 5.0",
		year: '1997',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pcfrance.bin`, size: 523837440 },
			cdrom: { url: `${DISC_HOST}/PCFRANCE.bin`, size: 607614976 },
		},
	},
	/** PC Premier 5.0 (1997) */
	pcpremier5: {
		name: "PC Premier 5.0",
		year: '1997',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pcpremier5.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pcpremier5_data.bin`, size: 322122240 },
			cdrom: { url: `${DISC_HOST}/PCPREMIER5.bin`, size: 429424640 },
		},
	},
	/** PC Premier 6.0 (1998) */
	pcpremier: {
		name: "PC Premier 6.0",
		year: '1998',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pcpremier.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pcpremier_data.bin`, size: 322122240 },
			cdrom: { url: `${DISC_HOST}/PCPREMIER.bin`, size: 607614976 },
		},
	},
	/** PC Real Madrid 99 (1999) */
	pcrm: {
		name: "PC Real Madrid 99",
		year: '1999',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pcrm.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pcrm_data.bin`, size: 322122240 },
			cdrom: { url: `${DISC_HOST}/PCRM.bin`, size: 437626880 },
		},
	},
	/** PC Real Madrid Basket '99 (1999) */
	pcrmbasket99: {
		name: "PC Real Madrid Basket '99",
		year: '1999',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pcrmbasket99.bin`, size: 523837440 },
			cdrom: { url: `${DISC_HOST}/PCRMB99.bin`, size: 434548736 },
		},
	},
	/** PC River (2000) */
	pcriver: {
		name: "PC River",
		year: '2000',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pcriver.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pcriver_data.bin`, size: 429496320 },
			cdrom: { url: `${DISC_HOST}/PCRIVER.bin`, size: 374956032 },
		},
	},
	/** Premier Manager 97 (1997) */
	pm97: {
		name: "Premier Manager 97",
		year: '1997',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pm97.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pm97_data.bin`, size: 322122240 },
			cdrom: { url: `${DISC_HOST}/PM97.bin`, size: 433385472 },
		},
	},
	/** Premier Manager 98 (1998) */
	pm98: {
		name: "Premier Manager 98",
		year: '1998',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pm98.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pm98_data.bin`, size: 322122240 },
			cdrom: { url: `${DISC_HOST}/PM98.bin`, size: 566358016 },
		},
	},
	/** Premier Manager 99 (1999) */
	pm99: {
		name: "Premier Manager 99",
		year: '1999',
		disks: {
			hda: { url: `${DISC_HOST}/win98_pm99.bin`, size: 523837440 },
			hdb: { url: `${DISC_HOST}/win98_pm99_data.bin`, size: 644244992 },
			cdrom: { url: `${DISC_HOST}/PM99.bin`, size: 617578496 },
		},
	},
} as const satisfies Record<string, IReferenceGame>;

/**
 * The titles this project actually needs today: the declared target
 * (6.0 and 7.0) plus one that changes sport.
 *
 * PC Basket 6.5 is on the list on purpose. The same shell shipped as
 * football, basketball and cycling, so comparing 6.5 against PC Fútbol
 * 6.0 shows which parts of the grammar survived a change of sport and
 * which had to move — which is the evidence we need for a shell that is
 * not welded to football.
 *
 * The rest of the catalogue stays declared rather than commented out:
 * data costs nothing to keep, and a future question ("what did the
 * Argentine 6.0 do here?") is then one flag away instead of an edit.
 */
export const DEFAULT_SELECTION: readonly string[] = ['pcf6', 'pcf7', 'pcbasket65'];

/** Where downloads land. Relative to the workspace root, and gitignored. */
export const REFERENCE_DIR = '.generate/reference';
