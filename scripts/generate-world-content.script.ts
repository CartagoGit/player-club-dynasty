#!/usr/bin/env bun
/**
 * Grow the shipped world into two playable divisions.
 *
 * The starter pack held six clubs and eleven players, which cannot satisfy the
 * acceptance of a playable league (audit a00001, stage 4). This generator adds
 * what is missing — clubs, squads, managers and staff — and leaves every
 * entity that already exists untouched, so identifiers, tests and saves that
 * refer to Aurora Quay keep meaning the same thing.
 *
 * Every value is derived from a hash of the entity's own identifier, so the
 * output is deterministic: running it twice produces byte-identical files, and
 * adding a club never reshuffles another one. All names are invented from
 * syllables; nothing is taken from a real competition, club or person.
 *
 *   bun scripts/generate-world-content.script.ts [--check]
 */

import { readFileSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

import { workspaceRoot } from './mcp-vertex-root.script.ts';

const DATA_DIR = join(workspaceRoot, 'game', 'content', 'starter-world', 'data');

const FIRST_DIVISION = 'competition:boreal-series';
const SECOND_DIVISION = 'competition:cinder-tier';
const DIVISION_SIZE = 20;
const SQUAD_SIZE = 22;
const HOME_COUNTRY = 'country:norvane';
const COUNTRIES = ['country:norvane', 'country:soldara', 'country:velmora'] as const;

interface IReference {
	readonly relationId: string;
	readonly targetExternalId: string;
	readonly required: boolean;
}

interface IEntity {
	externalId: string;
	kindId: string;
	values: Record<string, string>;
	references: IReference[];
}

interface IDocument {
	schemaVersion: number;
	entities: IEntity[];
}

// --- deterministic derivation ------------------------------------------------

/** FNV-1a over the identifier: the same entity always derives the same values. */
const seedOf = (key: string): number => {
	let hash = 0x811c9dc5;
	for (let index = 0; index < key.length; index++) {
		hash ^= key.charCodeAt(index);
		hash = Math.imul(hash, 0x01000193) >>> 0;
	}
	return hash >>> 0;
};

/** A stable stream of numbers for one entity, addressed by field name. */
const pick = (key: string, field: string, minimum: number, maximum: number): number => {
	const span = maximum - minimum + 1;
	return minimum + (seedOf(`${key}#${field}`) % span);
};

const choose = <T>(key: string, field: string, options: readonly T[]): T =>
	options[seedOf(`${key}#${field}`) % options.length] as T;

// --- invented vocabulary -----------------------------------------------------

const CLUB_PREFIXES = [
	'Aldervane', 'Bruneport', 'Calderwick', 'Drossmere', 'Elmgate', 'Fennhollow',
	'Glasmoor', 'Halbrook', 'Irenfell', 'Jarrowmoor', 'Kelderhaven', 'Lansford',
	'Merrowgate', 'Norhelm', 'Ostrey', 'Pellworth', 'Quarrowden', 'Ravenmoor',
	'Silverdown', 'Thornbeck', 'Umberlee', 'Valdreth', 'Westmarch', 'Ythwell',
	'Zennorbay', 'Ashenlock', 'Brightmere', 'Coldhaven', 'Dunhollow', 'Eastreach',
	'Farrowden', 'Grimsdale', 'Highmarsh', 'Ironvale', 'Larkspire', 'Marrowford',
	'Northgale', 'Oakensea', 'Pinehurst', 'Redstrand',
] as const;

const CLUB_SUFFIXES = [
	'Athletic', 'United', 'Rovers', 'Sporting', 'Wanderers', 'City', 'Town',
	'Albion', 'Rangers', 'Union',
] as const;

const CITIES = [
	'Myrglass', 'Eidrim', 'Coldwater', 'Ferrowgate', 'Halden', 'Iskvale',
	'Jorvenn', 'Kelmar', 'Lundry', 'Merrow', 'Norwick', 'Ostmoor', 'Pellin',
	'Quorrin', 'Renholt', 'Selkirn', 'Torvane', 'Ulmsted', 'Verdren', 'Wexholm',
] as const;

const GIVEN_STARTS = [
	'Al', 'Bre', 'Cor', 'Dar', 'El', 'Fen', 'Gar', 'Hal', 'Il', 'Jor', 'Kel',
	'Lir', 'Mar', 'Nev', 'Or', 'Per', 'Quen', 'Ral', 'Sor', 'Tev', 'Ul', 'Ver',
	'Wyn', 'Yar', 'Zel',
] as const;

const GIVEN_ENDS = ['an', 'en', 'is', 'or', 'us', 'ay', 'ek', 'in', 'ar', 'el'] as const;

const SURNAME_STARTS = [
	'Ash', 'Brenn', 'Cald', 'Dorn', 'Eskil', 'Farr', 'Grev', 'Holm', 'Ivarn',
	'Jarn', 'Kess', 'Lund', 'Morv', 'Nes', 'Orv', 'Pren', 'Rask', 'Sten',
	'Torv', 'Vask', 'Wren', 'Yll',
] as const;

const SURNAME_ENDS = [
	'ard', 'berg', 'dahl', 'eth', 'ford', 'gard', 'holt', 'ing', 'kvist',
	'lund', 'mark', 'ness', 'quist', 'rune', 'sen', 'thorn', 'vik', 'wall',
] as const;

/** Squad shape: three keepers, seven defenders, seven midfielders, five forwards. */
const POSITIONS = [
	...Array.from({ length: 3 }, () => 'goalkeeper'),
	...Array.from({ length: 2 }, () => 'centre-back'),
	...Array.from({ length: 2 }, () => 'full-back'),
	'centre-back',
	'full-back',
	'sweeper',
	...Array.from({ length: 3 }, () => 'central-midfielder'),
	...Array.from({ length: 2 }, () => 'wide-midfielder'),
	'defensive-midfielder',
	'attacking-midfielder',
	...Array.from({ length: 3 }, () => 'striker'),
	'winger',
	'second-striker',
] as const;

const STAFF_ROLES = ['scout', 'physio'] as const;

// --- document helpers --------------------------------------------------------

const read = (name: string): IDocument =>
	JSON.parse(readFileSync(join(DATA_DIR, `${name}.json`), 'utf8')) as IDocument;

const write = (name: string, document: IDocument): void => {
	document.entities.sort((left, right) => left.externalId.localeCompare(right.externalId, 'en'));
	writeFileSync(join(DATA_DIR, `${name}.json`), `${JSON.stringify(document, null, 2)}\n`);
};

const reference = (relationId: string, targetExternalId: string): IReference => ({
	relationId,
	targetExternalId,
	required: true,
});

const byId = (entities: readonly IEntity[]): Map<string, IEntity> =>
	new Map(entities.map((entity) => [entity.externalId, entity]));

// --- generation --------------------------------------------------------------

const clubId = (index: number): string => {
	const prefix = CLUB_PREFIXES[index % CLUB_PREFIXES.length] as string;
	const suffix = CLUB_SUFFIXES[Math.floor(index / CLUB_PREFIXES.length) % CLUB_SUFFIXES.length] as string;
	return `club:${prefix.toLowerCase()}-${suffix.toLowerCase()}`;
};

const buildClub = (id: string, tier: 1 | 2): IEntity => {
	const [, slug] = id.split(':');
	const words = (slug ?? '').split('-');
	const name = words
		.map((word) => word.charAt(0).toUpperCase() + word.slice(1))
		.join(' ');
	const prestigeFloor = tier === 1 ? 45 : 24;
	return {
		externalId: id,
		kindId: 'club',
		values: {
			'display-name': name,
			'short-name': (words[0] ?? name).replace(/^./, (character) => character.toUpperCase()),
			'founded-year': String(pick(id, 'founded', 1889, 1974)),
			city: choose(id, 'city', CITIES),
			prestige: String(pick(id, 'prestige', prestigeFloor, prestigeFloor + 24)),
			fanbase: String(pick(id, 'fanbase', prestigeFloor - 10, prestigeFloor + 20)),
		},
		references: [reference('country', HOME_COUNTRY)],
	};
};

const personName = (id: string): string =>
	`${choose(id, 'given-start', GIVEN_STARTS)}${choose(id, 'given-end', GIVEN_ENDS)} ` +
	`${choose(id, 'surname-start', SURNAME_STARTS)}${choose(id, 'surname-end', SURNAME_ENDS)}`;

const buildPlayer = (id: string, club: IEntity, slot: number): IEntity => {
	const prestige = Number(club.values.prestige ?? '40');
	const base = Math.max(24, Math.min(88, prestige + pick(id, 'quality', -14, 16)));
	const birthYear = pick(id, 'birth-year', 1988, 2008);
	return {
		externalId: id,
		kindId: 'person',
		values: {
			'display-name': personName(id),
			occupation: 'player',
			position: POSITIONS[slot % POSITIONS.length] as string,
			'birth-year': String(birthYear),
			'birth-date': `${birthYear}-${String(pick(id, 'birth-month', 1, 12)).padStart(2, '0')}-${String(pick(id, 'birth-day', 1, 28)).padStart(2, '0')}`,
			'contract-until': String(pick(id, 'contract', 2027, 2031)),
			'wage-weekly': String(pick(id, 'wage', 2, 40) * 500),
			technique: String(Math.max(20, Math.min(95, base + pick(id, 'technique', -8, 8)))),
			form: String(Math.max(20, Math.min(95, base + pick(id, 'form', -10, 10)))),
			physical: String(Math.max(20, Math.min(95, base + pick(id, 'physical', -9, 9)))),
			mental: String(Math.max(20, Math.min(95, base + pick(id, 'mental', -9, 9)))),
		},
		references: [
			reference('club', club.externalId),
			reference('country', choose(id, 'country', COUNTRIES)),
		],
	};
};

const buildManager = (id: string, club: IEntity): IEntity => ({
	externalId: id,
	kindId: 'person',
	values: {
		'display-name': personName(id),
		occupation: 'manager',
		'birth-year': String(pick(id, 'birth-year', 1965, 1992)),
		reputation: String(pick(id, 'reputation', 28, 78)),
	},
	references: [
		reference('club', club.externalId),
		reference('country', choose(id, 'country', COUNTRIES)),
	],
});

const buildStaff = (id: string, club: IEntity, role: string): IEntity => ({
	externalId: id,
	kindId: 'person',
	values: {
		'display-name': personName(id),
		occupation: role,
		'birth-year': String(pick(id, 'birth-year', 1962, 1996)),
		reputation: String(pick(id, 'reputation', 20, 70)),
	},
	references: [
		reference('club', club.externalId),
		reference('country', choose(id, 'country', COUNTRIES)),
	],
});

// --- composition -------------------------------------------------------------

const clubsDocument = read('clubs');
const peopleDocument = read('people');
const competitionsDocument = read('competitions');

const existingClubs = byId(clubsDocument.entities);
const existingPeople = byId(peopleDocument.entities);

/** Clubs already fielded by the first division keep their place in it. */
const firstDivision = competitionsDocument.entities.find(
	(entity) => entity.externalId === FIRST_DIVISION,
);
if (firstDivision === undefined) {
	throw new Error(`${FIRST_DIVISION} is missing from competitions.json`);
}

const firstDivisionClubs: string[] = firstDivision.references
	.filter((entry) => entry.relationId === 'participant')
	.map((entry) => entry.targetExternalId);

/** Same for the second division: its existing members must be read back, or a
 * second run would invent a whole new division instead of finding it full. */
const existingSecondDivision = competitionsDocument.entities.find(
	(entity) => entity.externalId === SECOND_DIVISION,
);
const secondDivisionClubs: string[] = (existingSecondDivision?.references ?? [])
	.filter((entry) => entry.relationId === 'participant')
	.map((entry) => entry.targetExternalId);

let generated = 0;
while (firstDivisionClubs.length < DIVISION_SIZE || secondDivisionClubs.length < DIVISION_SIZE) {
	const id = clubId(generated++);
	if (existingClubs.has(id)) continue;
	const tier: 1 | 2 = firstDivisionClubs.length < DIVISION_SIZE ? 1 : 2;
	const club = buildClub(id, tier);
	clubsDocument.entities.push(club);
	existingClubs.set(id, club);
	(tier === 1 ? firstDivisionClubs : secondDivisionClubs).push(id);
}

firstDivision.references = [
	...firstDivision.references.filter((entry) => entry.relationId !== 'participant'),
	...firstDivisionClubs.map((id) => reference('participant', id)),
];

const secondDivision: IEntity = existingSecondDivision ?? {
	externalId: SECOND_DIVISION,
	kindId: 'competition',
	values: {
		'display-name': 'Cinder Tier',
		format: 'round-robin',
		'edition-frequency': 'annual',
	},
	references: [],
};
secondDivision.references = [
	reference('country', HOME_COUNTRY),
	reference('rule-set', 'rules:domestic-league'),
	...secondDivisionClubs.map((id) => reference('participant', id)),
];
if (!competitionsDocument.entities.includes(secondDivision)) {
	competitionsDocument.entities.push(secondDivision);
}

/**
 * Players authored before positions and contracts existed keep every value
 * they already declare and gain only what is missing, so their identity, their
 * attributes and the tests that name them stay exactly as they were.
 */
const completePlayer = (player: IEntity, slot: number): void => {
	const id = player.externalId;
	const birthYear = Number(player.values['birth-year'] ?? String(pick(id, 'birth-year', 1988, 2008)));
	const defaults: Record<string, string> = {
		position: POSITIONS[slot % POSITIONS.length] as string,
		'birth-year': String(birthYear),
		'birth-date': `${birthYear}-${String(pick(id, 'birth-month', 1, 12)).padStart(2, '0')}-${String(pick(id, 'birth-day', 1, 28)).padStart(2, '0')}`,
		'contract-until': String(pick(id, 'contract', 2027, 2031)),
		'wage-weekly': String(pick(id, 'wage', 2, 40) * 500),
	};
	for (const [key, value] of Object.entries(defaults)) {
		player.values[key] ??= value;
	}
};

/** Every club fields a full squad, one manager and its support staff. */
for (const club of [...existingClubs.values()]) {
	const slug = club.externalId.split(':')[1] ?? club.externalId;
	const squad = peopleDocument.entities.filter(
		(person) =>
			person.values.occupation === 'player' &&
			person.references.some(
				(entry) => entry.relationId === 'club' && entry.targetExternalId === club.externalId,
			),
	);

	squad.forEach((player, slot) => completePlayer(player, slot));

	for (let slot = squad.length; slot < SQUAD_SIZE; slot++) {
		const id = `person:${slug}-p${String(slot + 1).padStart(2, '0')}`;
		if (existingPeople.has(id)) continue;
		const player = buildPlayer(id, club, slot);
		peopleDocument.entities.push(player);
		existingPeople.set(id, player);
	}

	const hasManager = peopleDocument.entities.some(
		(person) =>
			person.values.occupation === 'manager' &&
			person.references.some(
				(entry) => entry.relationId === 'club' && entry.targetExternalId === club.externalId,
			),
	);
	if (!hasManager) {
		const id = `person:${slug}-manager`;
		const manager = buildManager(id, club);
		peopleDocument.entities.push(manager);
		existingPeople.set(id, manager);
	}

	for (const role of STAFF_ROLES) {
		const id = `person:${slug}-${role}`;
		if (existingPeople.has(id)) continue;
		const member = buildStaff(id, club, role);
		peopleDocument.entities.push(member);
		existingPeople.set(id, member);
	}
}

write('clubs', clubsDocument);
write('people', peopleDocument);
write('competitions', competitionsDocument);

const counts = peopleDocument.entities.reduce<Record<string, number>>((totals, person) => {
	const occupation = person.values.occupation ?? 'unknown';
	totals[occupation] = (totals[occupation] ?? 0) + 1;
	return totals;
}, {});

process.stdout.write(
	`generate-world-content: ${clubsDocument.entities.length} clubs, ` +
		`${Object.entries(counts)
			.sort(([left], [right]) => left.localeCompare(right, 'en'))
			.map(([occupation, total]) => `${total} ${occupation}`)
			.join(', ')}, ` +
		`${firstDivisionClubs.length} in the first division and ${secondDivisionClubs.length} in the second.\n`,
);
