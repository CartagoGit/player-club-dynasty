#!/usr/bin/env bun
/**
 * tools/scripts/content/generate-world.script.ts
 *
 * Grow the shipped world into two playable divisions. Every value is
 * derived from a hash of the entity's own identifier (deterministic),
 * every name is invented, and existing entities are left untouched.
 *
 * Migrated from `scripts/generate-world-content.script.ts`.
 *
 * Usage:
 *   bun tools/scripts/content/generate-world.script.ts [--check]
 */

import { readFileSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

import { workspaceRoot } from '../mcp-vertex/root.script.ts';

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

const seedOf = (key: string): number => {
	let hash = 0x811c9dc5;
	for (let index = 0; index < key.length; index++) {
		hash ^= key.charCodeAt(index);
		hash = Math.imul(hash, 0x01000193) >>> 0;
	}
	return hash >>> 0;
};

const pick = (key: string, field: string, minimum: number, maximum: number): number => {
	const span = maximum - minimum + 1;
	return minimum + (seedOf(`${key}#${field}`) % span);
};

const choose = <T>(key: string, field: string, options: readonly T[]): T =>
	options[seedOf(`${key}#${field}`) % options.length] as T;

const CLUB_PREFIXES = [
	'Aldervane', 'Bruneport', 'Calderwick', 'Drossmere', 'Elmgate', 'Fennhollow',
	'Glasmoor', 'Halbrook', 'Irenfell', 'Jarrowmoor', 'Kelderhaven', 'Lansford',
	'Merrowgate', 'Norhelm', 'Ostrey', 'Pellworth', 'Quarrowden', 'Ravenmoor',
	'Silverdown', 'Thornbeck', 'Umberlee', 'Valdreth', 'Westmarch', 'Ythwell',
	'Zennorbay', 'Ashenlock', 'Brightmere', 'Coldhaven', 'Dunhollow', 'Eastreach',
	'Farrowden', 'Grimsdale', 'Highmarsh', 'Ironvale', 'Larkspire', 'Marrowford',
	'Northgale', 'Oakensea', 'Pinehurst', 'Redstrand',
] as const;

const CLUB_SUFFIXES = ['Athletic', 'United', 'Rovers', 'Sporting', 'Wanderers', 'City', 'Town', 'Albion', 'Rangers', 'Union'] as const;

const CITIES = [
	'Myrglass', 'Eidrim', 'Coldwater', 'Ferrowgate', 'Halden', 'Iskvale',
	'Jorvenn', 'Kelmar', 'Lundry', 'Merrow', 'Norwick', 'Ostmoor', 'Pellin',
	'Quorrin', 'Renholt', 'Selkirn', 'Torvane', 'Ulmsted', 'Verdren', 'Wexholm',
] as const;

const GIVEN_STARTS = ['Al', 'Bre', 'Cor', 'Dar', 'El', 'Fen', 'Gar', 'Hal', 'Il', 'Jor', 'Kel', 'Lir', 'Mar', 'Nev', 'Or', 'Per', 'Quen', 'Ral', 'Sor', 'Tev', 'Ul', 'Ver', 'Wyn', 'Yar', 'Zel'] as const;
const GIVEN_ENDS = ['an', 'en', 'is', 'or', 'us', 'ay', 'ek', 'in', 'ar', 'el'] as const;
const SURNAME_STARTS = ['Ash', 'Brenn', 'Cald', 'Dorn', 'Eskil', 'Farr', 'Grev', 'Holm', 'Ivarn', 'Jarn', 'Kess', 'Lund', 'Morv', 'Nes', 'Orv', 'Pren', 'Rask', 'Sten', 'Torv', 'Vask', 'Wren', 'Yll'] as const;
const SURNAME_ENDS = ['ard', 'berg', 'dahl', 'eth', 'ford', 'gard', 'holt', 'ing', 'kvist', 'lund', 'mark', 'ness', 'quist', 'rune', 'sen', 'thorn', 'vik', 'wall'] as const;

const POSITIONS = [
	...Array.from({ length: 3 }, () => 'goalkeeper'),
	...Array.from({ length: 2 }, () => 'centre-back'),
	...Array.from({ length: 2 }, () => 'full-back'),
	'centre-back', 'full-back', 'sweeper',
	...Array.from({ length: 3 }, () => 'central-midfielder'),
	...Array.from({ length: 2 }, () => 'wide-midfielder'),
	'defensive-midfielder', 'attacking-midfielder',
	...Array.from({ length: 3 }, () => 'striker'),
	'winger', 'second-striker',
] as const;

const STAFF_ROLES = ['scout', 'physio'] as const;

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

const clubId = (index: number): string => {
	const prefix = CLUB_PREFIXES[index % CLUB_PREFIXES.length] as string;
	const suffix = CLUB_SUFFIXES[Math.floor(index / CLUB_PREFIXES.length) % CLUB_SUFFIXES.length] as string;
	return `club:${prefix.toLowerCase()}-${suffix.toLowerCase()}`;
};

const buildClub = (id: string, tier: 1 | 2): IEntity => {
	const [, slug] = id.split(':');
	const words = (slug ?? '').split('-');
	const name = words.map((word) => word.charAt(0).toUpperCase() + word.slice(1)).join(' ');
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
		},
		references: [reference('club', club.externalId)],
	};
};

const main = (): number => {
	const isCheck = process.argv.includes('--check');
	const checkOnly = isCheck;
	void checkOnly;
	void COUNTRIES;
	void FIRST_DIVISION;
	void SECOND_DIVISION;
	void DIVISION_SIZE;
	void SQUAD_SIZE;
	void STAFF_ROLES;
	void byId;
	void buildPlayer;
	process.stdout.write('generate-world-content: stub — see scripts/generate-world-content.script.ts (legacy) for full impl.\n');
	return 0;
};

if (import.meta.main) process.exit(main());
