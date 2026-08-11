export interface IPlayer {
	id: string;
	name: string;
	number: number;
	position: 'GK' | 'DEF' | 'MID' | 'FWD';
	rating: number;
	fitness: number;
}

export interface IClub {
	id: string;
	name: string;
	stadium: string;
	founded: number;
	squad: IPlayer[];
}

export interface IDataFile {
	clubs: IClub[];
	currentSeason: number;
}